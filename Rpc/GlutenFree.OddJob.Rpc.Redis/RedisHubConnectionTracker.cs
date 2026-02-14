using System.Collections.Concurrent;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    public class RedisHubConnectionTracker<TKey,TTimedCache> : IKeyedTimedCacheStore<RedisHubTimedCache<TKey,TTimedCache>,TKey> where TTimedCache : ITimedCache<TKey>, new()
    {
        private ConnectionMultiplexer _multiplexer;

        public static ConcurrentDictionary<string, RedisHubTimedCache<TKey, TTimedCache>>
            InternalDictionary { get; } = new ConcurrentDictionary<string, RedisHubTimedCache<TKey,TTimedCache>>();

        public RedisHubConnectionTracker(ConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
        }
        public RedisHubTimedCache<TKey,TTimedCache> GetOrCreate(string key)
        {
            return InternalDictionary.GetOrAdd(key,
                (k) => new RedisHubTimedCache<TKey,TTimedCache>(_multiplexer, k));
        }
    }
}