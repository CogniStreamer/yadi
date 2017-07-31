using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    public abstract class KeyedDataLoader<TKey, TReturn> : IKeyedDataLoader<TKey, TReturn>, IExecutableDataLoader
    {
        private readonly IDataLoaderContext _context;
        private ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> _batch = new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>();
        private readonly object _syncRoot = new object();

        internal KeyedDataLoader(IDataLoaderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected KeyedDataLoader(DataLoaderContext context)
            : this((IDataLoaderContext)context)
        {
        }

        public Task<TReturn> LoadAsync(TKey key)
        {
            lock (_syncRoot)
            {
                if (!Enumerable.Any(_batch)) _context.QueueExecutableDataLoader(this);
                return _batch.GetOrAdd(key, _ => new TaskCompletionSource<TReturn>()).Task;
            }
        }

        protected abstract Task<IReadOnlyDictionary<TKey, TReturn>> Fetch(IEnumerable<TKey> keys, CancellationToken token);

        async Task<Task> IExecutableDataLoader.ExecuteAsync(CancellationToken token)
        {
            ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> thisBatch;
            lock (_syncRoot)
            {
                thisBatch = _batch;
                _batch = new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>();
            }
            if (!thisBatch.Any()) return Task.FromResult(0);
            var data = await Fetch(thisBatch.Keys, token).ConfigureAwait(false);
            return Task.Run(() => { foreach (var kvp in thisBatch) kvp.Value.SetResult(data[kvp.Key]); }, token);
        }
    }
}