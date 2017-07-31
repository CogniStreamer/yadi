﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Yadi.Tests
{
    [TestFixture]
    public class KeyedDataLoaderTests
    {
        private Mock<IDataLoaderContext> _contextMock;
        private List<Tuple<Guid[], CancellationToken>> _fetchCalls;
        private KeyedDataLoader<Guid, Book> _loader;

        [SetUp]
        public void SetUp()
        {
            _contextMock = new Mock<IDataLoaderContext>();
            _fetchCalls = new List<Tuple<Guid[], CancellationToken>>();
            _loader = new KeyedDataLoader<Guid, Book>(
                _contextMock.Object,
                (keys, token) =>
                {
                    _fetchCalls.Add(new Tuple<Guid[], CancellationToken>(keys.ToArray(), token));

                    return Task.FromResult<IReadOnlyDictionary<Guid, Book>>(keys.Select(id => new Book
                    {
                        Id = id,
                        Title = $"SomeTitle",
                        Author = $"SomeAuthor"
                    }).ToDictionary(x => x.Id));
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
            var bookId = Guid.NewGuid();
            _loader.LoadAsync(bookId);
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
            var bookId = Guid.NewGuid();
            var loadTask = _loader.LoadAsync(bookId);
            Assert.That(loadTask.IsCompleted, Is.False);

            await await (_loader as IExecutableDataLoader).ExecuteAsync(token);
            Assert.That(loadTask.IsCompleted, Is.True);

            Assert.That(loadTask.Result.Id, Is.EqualTo(bookId));
            Assert.That(loadTask.Result.Title, Is.EqualTo("SomeTitle"));
            Assert.That(loadTask.Result.Author, Is.EqualTo("SomeAuthor"));

            Assert.That(_fetchCalls.Count, Is.EqualTo(1));
            Assert.That(_fetchCalls[0].Item1.Length, Is.EqualTo(1));
            Assert.That(_fetchCalls[0].Item1[0], Is.EqualTo(bookId));
            Assert.That(_fetchCalls[0].Item2, Is.EqualTo(token));
        }

        [Test]
        public async Task ExecuteAsync_LoadAsyncCalledMultipleTimes_ShouldResolveAllLoadAsyncTasks()
        {
            var bookIds = Enumerable.Range(0, 20).Select(_ => Guid.NewGuid()).ToArray();
            var loadTasks = bookIds.Select(bookId => _loader.LoadAsync(bookId)).ToArray();

            await await (_loader as IExecutableDataLoader).ExecuteAsync(CancellationToken.None);

            for (var i = 0; i < loadTasks.Length; i++)
            {
                Assert.That(loadTasks[i].IsCompleted, Is.True);
                Assert.That(loadTasks[i].Result.Id, Is.EqualTo(bookIds[i]));
            }

            Assert.That(_fetchCalls.Count, Is.EqualTo(1));
            Assert.That(_fetchCalls[0].Item1.Length, Is.EqualTo(bookIds.Length));
        }

        [Test]
        public async Task ExecuteAsync_LoadAsyncTwiceForSameId_ShouldOnlyFetchSingleIdButResolveBothLoadAsyncTasks()
        {
            var bookId = Guid.NewGuid();
            var task1 = _loader.LoadAsync(bookId);
            var task2 = _loader.LoadAsync(bookId);

            await await (_loader as IExecutableDataLoader).ExecuteAsync(CancellationToken.None);

            Assert.That(task1.IsCompleted, Is.True);
            Assert.That(task2.IsCompleted, Is.True);

            Assert.That(_fetchCalls.Count, Is.EqualTo(1));
            Assert.That(_fetchCalls[0].Item1.Length, Is.EqualTo(1));
            Assert.That(_fetchCalls[0].Item1[0], Is.EqualTo(bookId));
        }

        private class Book
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
        }
    }
}
