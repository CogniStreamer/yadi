using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    internal class KeyedDataLoader<TKey, TReturn> : IKeyedDataLoader<TKey, TReturn>, IExecutableDataLoader
    {
        private readonly IDataLoaderContext _context;
        private readonly Func<IEnumerable<TKey>, CancellationToken, Task<IReadOnlyDictionary<TKey, TReturn>>> _fetch;
        private ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> _batch = new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>();
        private readonly object _syncRoot = new object();

        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<TKey>, CancellationToken, Task<IReadOnlyDictionary<TKey, TReturn>>> fetch)
        {
            _context = context;
            _fetch = fetch;
        }

        public Task<TReturn> LoadAsync(TKey key)
        {
            lock (_syncRoot)
            {
                if (!_batch.Any()) _context.QueueExecutableDataLoader(this);
                return _batch.GetOrAdd(key, _ => new TaskCompletionSource<TReturn>()).Task;
            }
        }

        async Task<Task> IExecutableDataLoader.ExecuteAsync(CancellationToken token)
        {
            ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>> thisBatch;
            lock (_syncRoot)
            {
                thisBatch = _batch;
                _batch = new ConcurrentDictionary<TKey, TaskCompletionSource<TReturn>>();
            }
            if (!thisBatch.Any()) return Task.FromResult(0);
            var data = await _fetch(thisBatch.Keys, token).ConfigureAwait(false);
            return Task.Run(() => { foreach (var kvp in thisBatch) kvp.Value.SetResult(data[kvp.Key]); }, token);
        }
    }
}