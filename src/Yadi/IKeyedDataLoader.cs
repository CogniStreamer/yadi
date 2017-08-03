using System.Threading.Tasks;

namespace Yadi
{
    public interface IKeyedDataLoader<in TKey, TReturn>
    {
        Task<TReturn> LoadAsync(TKey key);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, in TKey3, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, in TKey3, in TKey4, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, in TKey3, in TKey4, in TKey5, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, in TKey3, in TKey4, in TKey5, in TKey6, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6);
    }

    public interface IKeyedDataLoader<in TKey1, in TKey2, in TKey3, in TKey4, in TKey5, in TKey6, in TKey7, TReturn>
    {
        Task<TReturn> LoadAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, TKey6 key6, TKey7 key7);
    }
}