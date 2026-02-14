using MagicOnion.Server;
using MagicOnion.Utils;

namespace OddJob.Rpc.Server.Redis
{
    public static class StreamingConstants
    {
        public static readonly string RedisString =
            "OddJob.StreamingHub.QueueCache?queueName=";
    }
}