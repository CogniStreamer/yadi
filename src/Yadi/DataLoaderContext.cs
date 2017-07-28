using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    public class DataLoaderContext
    {
        private Queue<IExecutableDataLoader> _executableDataLoaders = new Queue<IExecutableDataLoader>();

        internal void QueueExecutableDataLoader(IExecutableDataLoader dataLoader)
            => _executableDataLoaders.Enqueue(dataLoader);

        public async Task Complete(CancellationToken token)
        {
            var thisQueue = Interlocked.Exchange(ref _executableDataLoaders, new Queue<IExecutableDataLoader>());
            while (thisQueue.Any()) await thisQueue.Dequeue().ExecuteAsync(token).ConfigureAwait(false);
        }
    }
}
