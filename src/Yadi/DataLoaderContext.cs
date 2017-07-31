using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Yadi.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Yadi
{
    public sealed class DataLoaderContext : IDataLoaderContext
    {
        private readonly ConcurrentDictionary<TypeInfo, object> _cache = new ConcurrentDictionary<TypeInfo, object>();
        private readonly ConcurrentDictionary<Tuple<TypeInfo, TypeInfo>, object> _keyedCache = new ConcurrentDictionary<Tuple<TypeInfo, TypeInfo>, object>();
        private Queue<IExecutableDataLoader> _executableDataLoaders = new Queue<IExecutableDataLoader>();

        void IDataLoaderContext.QueueExecutableDataLoader(IExecutableDataLoader dataLoader)
            => _executableDataLoaders.Enqueue(dataLoader);

        public IDataLoader<TReturn> GetOrCreateLoader<TReturn>(Func<CancellationToken, Task<TReturn>> fetch)
            => (IDataLoader<TReturn>)_cache.GetOrAdd(typeof(TReturn).GetTypeInfo(), _ => new DataLoader<TReturn>(this, fetch));

        public IKeyedDataLoader<TKey, TReturn> GetOrCreateLoader<TKey, TReturn>(Func<IEnumerable<TKey>, CancellationToken, Task<IReadOnlyDictionary<TKey, TReturn>>> fetch)
            => (IKeyedDataLoader<TKey, TReturn>) _keyedCache.GetOrAdd(new Tuple<TypeInfo, TypeInfo>(typeof(TKey).GetTypeInfo(), typeof(TReturn).GetTypeInfo()),
                _ => new KeyedDataLoader<TKey, TReturn>(this, fetch));
        
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
