using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<object, object> _cache = new ConcurrentDictionary<object, object>();
        private Queue<IExecutableDataLoader> _executableDataLoaders = new Queue<IExecutableDataLoader>();

        void IDataLoaderContext.QueueExecutableDataLoader(IExecutableDataLoader dataLoader)
            => _executableDataLoaders.Enqueue(dataLoader);

        public IDataLoader<TReturn> GetOrCreateLoader<TReturn>(Func<CancellationToken, Task<TReturn>> fetch)
            => (IDataLoader<TReturn>) _cache.GetOrAdd(fetch, _ => new DataLoader<TReturn>(this, fetch));

        public IKeyedDataLoader<TKey, TReturn> GetOrCreateLoader<TKey, TReturn>(Func<IEnumerable<TKey>, CancellationToken, Task<IReadOnlyDictionary<TKey, TReturn>>> fetch)
            => (IKeyedDataLoader<TKey, TReturn>) _cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TReturn> GetOrCreateLoader<TKey1, TKey2, TReturn>(Func<IEnumerable<(TKey1, TKey2)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TKey3, TReturn> GetOrCreateLoader<TKey1, TKey2, TKey3, TReturn>(Func<IEnumerable<(TKey1, TKey2, TKey3)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TKey3, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TKey3, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TReturn> GetOrCreateLoader<TKey1, TKey2, TKey3, TKey4, TReturn>(Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn> GetOrCreateLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn>(Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn> GetOrCreateLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn>(Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TReturn>(this, fetch));

        public IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn> GetOrCreateLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn>(Func<IEnumerable<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7)>, CancellationToken, Task<IReadOnlyDictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7), TReturn>>> fetch)
            => (IKeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn>)_cache.GetOrAdd(fetch, _ => new KeyedDataLoader<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6, TKey7, TReturn>(this, fetch));

        public async Task Complete(CancellationToken token)
        {
            var tasks = new List<Task>();
            while (tasks.Any() || _executableDataLoaders.Any())
            {
                if (_executableDataLoaders.Any())
                {
                    var queue = Interlocked.Exchange(ref _executableDataLoaders, new Queue<IExecutableDataLoader>());
                    while (queue.Any()) tasks.Add(await queue.Dequeue().ExecuteAsync(token).ConfigureAwait(false));
                }

                tasks.Remove(await Task.WhenAny(tasks).ConfigureAwait(false));
            }
        }
    }
}
