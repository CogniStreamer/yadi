using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    public abstract class DataLoader<TReturn> : IDataLoader<TReturn>, IExecutableDataLoader
    {
        private readonly IDataLoaderContext _context;
        private TaskCompletionSource<TReturn> _query = null;
        private readonly object _syncRoot = new object();

        internal DataLoader(IDataLoaderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected DataLoader(DataLoaderContext context)
            : this((IDataLoaderContext)context)
        {
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

        protected abstract Task<TReturn> Fetch(CancellationToken token);

        async Task<Task> IExecutableDataLoader.ExecuteAsync(CancellationToken token)
        {
            TaskCompletionSource<TReturn> thisQuery;
            lock (_syncRoot)
            {
                thisQuery = _query;
                _query = null;
            }
            if (thisQuery == null) return Task.FromResult(0);
            var data = await Fetch(token).ConfigureAwait(false);
            return Task.Run(() => thisQuery.SetResult(data), token);
        }
    }
}
