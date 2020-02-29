using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MessagePack;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    /// <summary>
    /// Provides a Redis Synchronization
    /// On top of TimedCache
    /// This allows for multiple subscribers.
    /// </summary>
    /// <typeparam name="TKey">The type of key to cache</typeparam>
    public class RedisHubTimedCache<TKey,TTimedCache> : ITimedCache<TKey> where TTimedCache :  ITimedCache<TKey>,new()
    {
        public bool ShouldNotRandomize
        {
            get { return false; }
        }
        public string QueueName { get; protected set; }
        public ISubscriber Subscriber { get; set; }
        
        public string QueueStr { get; protected set; }
        
        public ConcurrentDictionary<string,TTimedCache>TimedCaches = new ConcurrentDictionary<string, TTimedCache>();

        public RedisHubTimedCache(ConnectionMultiplexer multiplexer,
            string queueName)
        {
            QueueName = queueName;
            QueueStr = StreamingConstants.RedisString +
                       queueName;

            Subscriber = multiplexer.GetSubscriber();
            Subscriber.Subscribe(
                new RedisChannel(QueueStr, RedisChannel.PatternMode.Literal),
                (ch, val) => HandleEvent(ch, val, queueName, TimedCaches));
        }

        public void Freshen(TKey guid, DateTime expiresAt)
        {
            
            var tc =
                TimedCaches.GetOrAdd(QueueName, s => new TTimedCache());
            tc.Freshen(guid,expiresAt);
            Subscriber.Publish(
                new RedisChannel(QueueStr, RedisChannel.PatternMode.Literal),
                GetPayload(guid, expiresAt));
        }
        
        public byte[] GetPayload(TKey connectionId, DateTime expiresAt)
        {
            var toSer = new RedisHubFreshenData<TKey>()
                {ConnectionId = connectionId, ExpiresAt = expiresAt};
            var bytes = MessagePackSerializer.Serialize(toSer); 
            return bytes;
        }

        private static void HandleEvent(RedisChannel arg1, RedisValue arg2,string queueName, ConcurrentDictionary<string,TTimedCache>timedCaches)
        {
            byte[] bytes = arg2;
            var datas =
                MessagePackSerializer.Deserialize<RedisHubFreshenData<TKey>>(bytes);
            var tc =
                timedCaches.GetOrAdd(queueName, s => new TTimedCache());
            tc.Freshen(datas.ConnectionId, datas.ExpiresAt);
        }

        public TKey[] GetItems()
        {
            var tc =
                TimedCaches.GetOrAdd(QueueName, s => new TTimedCache());
            return tc.GetItems();
        }
    }
}