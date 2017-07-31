using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    internal class DataLoader<TReturn> : IDataLoader<TReturn>, IExecutableDataLoader
    {
        private readonly IDataLoaderContext _context;
        private readonly Func<CancellationToken, Task<TReturn>> _fetch;
        private TaskCompletionSource<TReturn> _query = null;
        private readonly object _syncRoot = new object();

        public DataLoader(IDataLoaderContext context, Func<CancellationToken, Task<TReturn>> fetch)
        {
            _context = context;
            _fetch = fetch;
        }

        public Task<TReturn> LoadAsync()
        {
            lock (_syncRoot)
            {
                if (_query == null)
                {
                    _context.QueueExecutableDataLoader(this);
                    _query = new TaskCompletionSource<TReturn>();
                }
                return _query.Task;
            }
        }

        async Task<Task> IExecutableDataLoader.ExecuteAsync(CancellationToken token)
        {
            TaskCompletionSource<TReturn> thisQuery;
            lock (_syncRoot)
            {
                thisQuery = _query;
                _query = null;
            }
            if (thisQuery == null) return Task.FromResult(0);
            var data = await _fetch(token).ConfigureAwait(false);
            return Task.Run(() => thisQuery.SetResult(data), token);
        }
    }
}
