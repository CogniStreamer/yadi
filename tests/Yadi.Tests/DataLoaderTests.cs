using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using NUnit.Framework;

namespace Yadi.Tests
{
    [TestFixture]
    public class DataLoaderTests
    {
        private Mock<IDataLoaderContext> _contextMock;
        private List<CancellationToken> _fetchCalls;
        private DataLoader<Book[]> _loader;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IDataLoaderContext>();
            _fetchCalls = new EditableList<CancellationToken>();
            _loader = new DataLoader<Book[]>(
                _contextMock.Object,
                token =>
                {
                    _fetchCalls.Add(token);

                    return Task.FromResult(new[]
                    {
                        new Book
                        {
                            Id = Guid.NewGuid(),
                            Title = "Title 1",
                            Author = "Author 1"
                        },
                        new Book
                        {
                            Id = Guid.NewGuid(),
                            Title = "Title 2",
                            Author = "Author 2"
                        }
                    });
                });
        }

        [Test]
        public async Task Creation_NothingLoaded_ShouldNotQueueLoaderOnContext()
        {
            await (_loader as IExecutableDataLoader).ExecuteAsync(CancellationToken.None);
            _contextMock.Verify(x => x.QueueExecutableDataLoader(It.IsAny<IExecutableDataLoader>()), Times.Never);
        }

        [Test]
        public void LoadAsync_ShouldQueueLoaderAndCompleteTask()
        {
            _loader.LoadAsync();
            _contextMock.Verify(x => x.QueueExecutableDataLoader(_loader), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_NothingToLoad_ShouldNotCallFetchMethod()
        {
            await await (_loader as IExecutableDataLoader).ExecuteAsync(CancellationToken.None);
            Assert.That(_fetchCalls, Is.Empty);
        }

        [Test]
        public async Task ExecuteAsync_LoadAsyncCalled_ShouldResolveLoadAsyncTask()
        {
            var token = new CancellationTokenSource().Token;
            var loadTask = _loader.LoadAsync();
            Assert.That(loadTask.IsCompleted, Is.False);

            await await (_loader as IExecutableDataLoader).ExecuteAsync(token);
            Assert.That(loadTask.IsCompleted, Is.True);

            Assert.That(loadTask.Result.Length, Is.EqualTo(2));
            Assert.That(_fetchCalls.Count, Is.EqualTo(1));
            Assert.That(_fetchCalls[0], Is.EqualTo(token));
        }

        [Test]
        public async Task ExecuteAsync_LoadAsyncCalledMultipleTimes_ShouldOnlyFetchOnceAndResolveAllLoadAsyncTasks()
        {
            var token = new CancellationTokenSource().Token;
            var loadTasks = Enumerable.Range(0, 20).Select(_ => _loader.LoadAsync()).ToArray();

            await await (_loader as IExecutableDataLoader).ExecuteAsync(token);

            Assert.That(_fetchCalls.Count, Is.EqualTo(1));
            Assert.That(_fetchCalls[0], Is.EqualTo(token));

            foreach (var task in loadTasks)
            {
                Assert.That(task.IsCompleted, Is.True);
                Assert.That(task.Result.Length, Is.EqualTo(2));
            }
        }

        public class Book
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }
    }
}
