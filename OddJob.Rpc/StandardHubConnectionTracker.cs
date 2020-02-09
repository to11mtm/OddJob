using System.Collections.Concurrent;

namespace OddJob.RpcServer
{
    public sealed class StandardHubConnectionTracker<T> : IKeyedTimedCacheStore<TimedCache<T>,T>
    {

        public static ConcurrentDictionary<string, TimedCache<T>>
            InternalDictionary { get; } = new ConcurrentDictionary<string, TimedCache<T>>();
        public TimedCache<T> GetOrCreate(string key)
        {
            return InternalDictionary.GetOrAdd(key, (k) => new TimedCache<T>());
        }
    }
}