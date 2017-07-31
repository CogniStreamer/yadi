using System.Threading.Tasks;

namespace Yadi
{
    public interface IDataLoader<TReturn>
    {
        Task<TReturn> LoadAsync();
    }
}
