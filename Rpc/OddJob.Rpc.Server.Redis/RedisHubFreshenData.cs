using System;
using MessagePack;

namespace OddJob.Rpc.Server.Redis
{
    [MessagePackObject]
    public class RedisHubFreshenData
    {
        [MessagePack.Key(1)]
        public Guid TheGuid { get; set; }
        [MessagePack.Key(2)]
        public DateTime ExpiresAt { get; set; }
    }
}