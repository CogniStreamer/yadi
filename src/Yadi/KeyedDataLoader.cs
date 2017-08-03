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
            return Task.Run(() =>
            {
                foreach (var kvp in thisBatch)
                {
                    var value = default(TReturn);
                    data?.TryGetValue(kvp.Key, out value);
                    kvp.Value.SetResult(value);
                }
            }, token);
        }
    }

    internal class KeyedDataLoader<TKey1, TKey2, TReturn> : KeyedDataLoader<(TKey1, TKey2), TReturn>, IKeyedDataLoader<TKey1, TKey2, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2) => LoadAsync((key1, key2));
    }

    internal class KeyedDataLoader<TKey1, TKey2, TKey3, TReturn> : KeyedDataLoader<(TKey1, TKey2, TKey3), TReturn>, IKeyedDataLoader<TKey1, TKey2, TKey3, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2, TKey3)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3) => LoadAsync((key1, key2, key3));
    }

    internal class KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TReturn> : KeyedDataLoader<(TKey1, TKey2, TKey3, TKey4), TReturn>, IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4) => LoadAsync((key1, key2, key3, key4));
    }

    internal class KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn> : KeyedDataLoader<(TKey1, TKey2, TKey3, TKey4, TKey5), TReturn>, IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5) => LoadAsync((key1, key2, key3, key4, key5));
    }

    internal class KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn> : KeyedDataLoader<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), TReturn>, IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6) => LoadAsync((key1, key2, key3, key4, key5, key6));
    }

    internal class KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn> : KeyedDataLoader<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7), TReturn>, IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn>
    {
        public KeyedDataLoader(IDataLoaderContext context, Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7), TReturn>>> fetch) : base(context, fetch) { }
        public Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7) => LoadAsync((key1, key2, key3, key4, key5, key6, key7));
    }
}