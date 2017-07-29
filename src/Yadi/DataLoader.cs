using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    public abstract class DataLoader<TKey, TReturn> : IDataLoader<TKey, TReturn>, IExecutableDataLoader
    {
        private readonly IDataLoaderContext _context;
        private ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> _batch = new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>();
        private readonly object _syncRoot = new object();

        internal DataLoader(IDataLoaderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected DataLoader(DataLoaderContext context)
            : this((IDataLoaderContext)context)
        {
        }

        public Task<TReturn> LoadAsync(TKey key)
        {
            lock (_syncRoot)
            {
                if (!_batch.Any()) _context.QueueExecutableDataLoader(this);
                return _batch.GetOrAdd(key, _ => new TaskCompletionSource<TReturn>()).Task;
            }
        }

        protected abstract Task<IReadOnlyDictionary<TKey, TReturn>> Fetch(IEnumerable<TKey> keys, CancellationToken token);

        async Task IExecutableDataLoader.ExecuteAsync(CancellationToken token)
        {
            ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> thisBatch;
            lock (_syncRoot) thisBatch = Interlocked.Exchange(ref _batch, new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>());
            if (!thisBatch.Any()) return;
            var data = await Fetch(thisBatch.Keys, token).ConfigureAwait(false);
            foreach (var kvp in thisBatch) kvp.Value.SetResult(data[kvp.Key]);
        }
    }
}
