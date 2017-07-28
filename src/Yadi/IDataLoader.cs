using System.Threading.Tasks;

namespace Yadi
{
    public interface IDataLoader<in TKey, TReturn>
    {
        Task<TReturn> LoadAsync(TKey key);
    }
}
