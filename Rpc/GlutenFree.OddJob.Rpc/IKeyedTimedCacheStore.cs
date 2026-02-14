namespace OddJob.RpcServer
{
    public interface IKeyedTimedCacheStore<TCache,T> where TCache:ITimedCache<T>
    {
        TCache GetOrCreate(string key);
    }
}