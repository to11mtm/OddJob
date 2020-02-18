using System.Collections.Concurrent;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    public class RedisHubConnectionTracker<TKey> : IKeyedTimedCacheStore<RedisHubTimedCache<TKey>,TKey>
    {
        private ConnectionMultiplexer _multiplexer;

        public static ConcurrentDictionary<string, RedisHubTimedCache<TKey>>
            InternalDictionary { get; } = new ConcurrentDictionary<string, RedisHubTimedCache<TKey>>();

        public RedisHubConnectionTracker(ConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
        }
        public RedisHubTimedCache<TKey> GetOrCreate(string key)
        {
            return InternalDictionary.GetOrAdd(key,
                (k) => new RedisHubTimedCache<TKey>(_multiplexer, k));
        }
    }
}