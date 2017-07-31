using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Yadi.Tests
{
    [TestFixture]
    public class DataLoaderContextTests
    {
        [Test]
        public async Task Complete_WithoutQueuedLoaders_ShouldNotThrow()
        {
            var context = new DataLoaderContext();
            await context.Complete(CancellationToken.None);
        }

        [Test]
        public async Task Complete_WithOneDataLoader_ShouldCallExecuteAsyncMethod()
        {
            var token = new CancellationTokenSource().Token;
            var context = new DataLoaderContext();
            var dataLoaderMock = new Mock<IExecutableDataLoader>();
            (context as IDataLoaderContext).QueueExecutableDataLoader(dataLoaderMock.Object);
            await context.Complete(token);

            dataLoaderMock.Verify(x => x.ExecuteAsync(token), Times.Once);
        }

        [Test]
        public async Task Complete_WithMultipleDataLoaders_ShouldCallAllExecuteAsyncMethods()
        {
            var token = new CancellationTokenSource().Token;
            var context = new DataLoaderContext();
            var dataLoaderMocks = Enumerable.Range(0, 20).Select(_ => new Mock<IExecutableDataLoader>()).ToList();
            dataLoaderMocks.ForEach(mock => (context as IDataLoaderContext).QueueExecutableDataLoader(mock.Object));
            await context.Complete(token);

            dataLoaderMocks.ForEach(mock => mock.Verify(x => x.ExecuteAsync(token), Times.Once));
        }

        [Test]
        public async Task Complete_WithMultipleDataLoaders_ShouldClearQueue()
        {
            var token = new CancellationTokenSource().Token;
            var context = new DataLoaderContext();
            var dataLoaderMocks = Enumerable.Range(0, 20).Select(_ => new Mock<IExecutableDataLoader>()).ToList();
            dataLoaderMocks.ForEach(mock => (context as IDataLoaderContext).QueueExecutableDataLoader(mock.Object));
            await context.Complete(token);
            dataLoaderMocks.ForEach(mock => mock.Verify(x => x.ExecuteAsync(token), Times.Once));

            await context.Complete(token);
            await context.Complete(token);
            dataLoaderMocks.ForEach(mock => mock.Verify(x => x.ExecuteAsync(token), Times.Once));
        }
    }
}
