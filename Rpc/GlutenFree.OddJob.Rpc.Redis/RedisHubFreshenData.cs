using System;
using MessagePack;

namespace OddJob.Rpc.Server.Redis
{
    [MessagePackObject]
    public class RedisHubFreshenData<T>
    {
        [MessagePack.Key(1)]
        public T ConnectionId { get; set; }
        [MessagePack.Key(2)]
        public DateTime ExpiresAt { get; set; }
    }
}