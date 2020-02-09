using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Server.Hubs;
using MessagePack;
using OddJob.RpcServer;
using StackExchange.Redis;

namespace OddJob.Rpc.Server.Redis
{
    
    public class
        ClusteredStreamingJobCreationServer : BaseStreamingJobCreationServer<RedisHubTimedCache>
    { 
        public ClusteredStreamingJobCreationServer(
            ISerializedJobQueueAdder jobQueueAdder, RedisHubConnectionTracker timeCache) : base(jobQueueAdder, timeCache)
        {
        }

    }
}