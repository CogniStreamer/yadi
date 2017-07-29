using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Yadi.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Yadi
{
    public sealed class DataLoaderContext : IDataLoaderContext
    {
        private Queue<IExecutableDataLoader> _executableDataLoaders = new Queue<IExecutableDataLoader>();

        void IDataLoaderContext.QueueExecutableDataLoader(IExecutableDataLoader dataLoader)
            => _executableDataLoaders.Enqueue(dataLoader);

        public async Task Complete(CancellationToken token)
        {
            var thisQueue = Interlocked.Exchange(ref _executableDataLoaders, new Queue<IExecutableDataLoader>());
            while (thisQueue.Any()) await thisQueue.Dequeue().ExecuteAsync(token).ConfigureAwait(false);
        }
    }
}
