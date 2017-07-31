using System.Threading.Tasks;

namespace Yadi
{
    public interface IKeyedDataLoader<in TKey, TReturn>
    {
        Task<TReturn> LoadAsync(TKey key);
    }
}