using System.Threading;
using System.Threading.Tasks;

namespace Yadi
{
    internal interface IExecutableDataLoader
    {
        Task ExecuteAsync(CancellationToken token);
    }
}
