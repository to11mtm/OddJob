using System;
using System.Collections.Concurrent;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    public class RedisHubConnectionTracker : IKeyedTimedCacheStore<RedisHubTimedCache,Guid>
    {
        private ConnectionMultiplexer _multiplexer;

        public static ConcurrentDictionary<string, RedisHubTimedCache>
            InternalDictionary { get; } = new ConcurrentDictionary<string, RedisHubTimedCache>();

        public RedisHubConnectionTracker(ConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer;
        }
        public RedisHubTimedCache GetOrCreate(string key)
        {
            return InternalDictionary.GetOrAdd(key,
                (k) => new RedisHubTimedCache(_multiplexer, k));
        }
    }
}