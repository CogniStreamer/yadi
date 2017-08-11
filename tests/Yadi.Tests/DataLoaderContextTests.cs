using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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

        [Test]
        [Timeout(500)]
        public async Task Complete_SingleLoaderTask_ShouldNotBlock()
        {
            var context = new DataLoaderContext();
            var task = SingleLoaderTask(context);
            await context.Complete(CancellationToken.None);
            var result = await task;
            Assert.That(result, Is.EqualTo(7));
        }

        [Test]
        [Timeout(500)]
        [Ignore("This test fails because continuation is executed asynchrounously")]
        public async Task Complete_MultipleLoaderTask_ShouldNotBlock()
        {
            var context = new DataLoaderContext();
            var task = MultipleLoaderTask(context);
            await context.Complete(CancellationToken.None);
            var result = await task.ConfigureAwait(false);
            Assert.That(result, Is.EqualTo(9));
        }

        private static Task<int> SingleLoaderTask(DataLoaderContext context)
            => context.GetOrCreateLoader(token =>
                Task.Run(() =>
                {
                    Thread.Sleep(7);
                    return 7;
                }, token)).LoadAsync();

        private static Task<int> MultipleLoaderTask(DataLoaderContext context)
        {
            var task1 = context.GetOrCreateLoader(async token =>
            {
                await Task.Delay(20, token);
                return 4;
            }).LoadAsync();

            return task1.ContinueWith(async t1 =>
            {
                var a = t1.Result;

                var task2 = context.GetOrCreateLoader(async token =>
                {
                    await Task.Delay(20, token);
                    return 5;
                }).LoadAsync();

                return await task2.ContinueWith(t2 =>
                {
                    var b = t2.Result;

                    return a + b;
                }, TaskContinuationOptions.None);

            }, TaskContinuationOptions.None).Unwrap();
        }
    }
}
