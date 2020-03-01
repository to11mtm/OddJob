using System.Collections.Concurrent;

namespace OddJob.RpcServer
{
    public sealed class
        StandardHubConnectionTracker<T, TTimedCache> : IKeyedTimedCacheStore<
            TTimedCache, T> where TTimedCache : ITimedCache<T>, new()
    {

        public static ConcurrentDictionary<string, TTimedCache>
            InternalDictionary { get; } =
            new ConcurrentDictionary<string, TTimedCache>();

        public TTimedCache GetOrCreate(string key)
        {
            return InternalDictionary.GetOrAdd(key, (k) => new TTimedCache());
        }
    }
}