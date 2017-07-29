namespace Yadi
{
    internal interface IDataLoaderContext
    {
        void QueueExecutableDataLoader(IExecutableDataLoader dataLoader);
    }
}
