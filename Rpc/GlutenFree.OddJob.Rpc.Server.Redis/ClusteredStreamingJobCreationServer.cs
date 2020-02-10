using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Rpc.Server;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Redis;
using MagicOnion.Server.Hubs;
using MessagePack;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    [GroupConfiguration(typeof(RedisGroupRepositoryFactory))]
    public class
        ClusteredStreamingJobCreationServer : BaseStreamingJobCreationServer<RedisHubTimedCache>
    { 
        public ClusteredStreamingJobCreationServer(
            ISerializedJobQueueAdder jobQueueAdder, RedisHubConnectionTracker timeCache) : base(jobQueueAdder, timeCache)
        {
        }

    }
}