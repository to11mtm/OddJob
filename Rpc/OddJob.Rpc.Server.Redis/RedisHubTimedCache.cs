using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MessagePack;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    public class RedisHubTimedCache : ITimedCache<Guid>
    {
        public string QueueName { get; protected set; }
        public ISubscriber Subscriber { get; set; }

        public static ConcurrentDictionary<string,TimedCache<Guid>>TimedCaches = new ConcurrentDictionary<string, TimedCache<Guid>>();
        public RedisHubTimedCache(ConnectionMultiplexer multiplexer, string queueName)
        {
            QueueName = queueName;
            var queueStr =  StreamingConstants.RedisString+
                            queueName;
            
            Subscriber = multiplexer.GetSubscriber();
            Subscriber.Subscribe(queueStr,
                (ch, val) => HandleEvent(ch, val, queueName));
        }

        public void Freshen(Guid guid, DateTime expiresAt)
        {
            
            var tc =
                TimedCaches.GetOrAdd(QueueName, s => new TimedCache<Guid>());
            tc.Freshen(guid,expiresAt);
            Subscriber.Publish(
                new RedisChannel(QueueName, RedisChannel.PatternMode.Auto),
                GetPayload(guid, expiresAt));
        }
        
        public byte[] GetPayload(Guid guid, DateTime expiresAt)
        {
            var toSer = new RedisHubFreshenData()
                {TheGuid = guid, ExpiresAt = expiresAt};
            return MessagePackSerializer.Serialize(toSer);
        }

        private static void HandleEvent(RedisChannel arg1, RedisValue arg2,string queueName)
        {
            byte[] bytes = arg2;
            var datas =
                MessagePackSerializer.Deserialize<RedisHubFreshenData>(bytes);
            var tc =
                TimedCaches.GetOrAdd(queueName, s => new TimedCache<Guid>());
            tc.Freshen(datas.TheGuid, datas.ExpiresAt);
        }

        public Guid[] GetItems()
        {
            var tc =
                TimedCaches.GetOrAdd(QueueName, s => new TimedCache<Guid>());
            return tc.GetItems();
        }
    }
}