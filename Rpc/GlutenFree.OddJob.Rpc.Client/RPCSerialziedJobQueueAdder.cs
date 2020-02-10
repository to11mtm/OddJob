using System.Collections.Generic;
using System.Linq;
using GlutenFree.OddJob.Serializable;
using Grpc.Core;
using MagicOnion.Client;
using OddJob.RpcServer;

namespace OddJob.Rpc.Client
{
    public class RPCSerialziedJobQueueAdder : ISerializedJobQueueAdder
    {
        private RpcClientConfiguration _conf;
        private IRpcJobCreator _client;
        private GRPCChannelPool _pool;
        public RPCSerialziedJobQueueAdder(GRPCChannelPool pool, RpcClientConfiguration conf)
        {
            _conf = conf;
            _pool = pool;
            Create();
        }
        public void Create()
        {
            _client = MagicOnionClient.Create<IRpcJobCreator>(
                new DefaultCallInvoker(_pool.GetChannel(_conf))
                , UnregisteredSerializerOptions.Instance,
                _conf.Filters.ToArray());
        }

        public  void AddJob(SerializableOddJob jobData)
        {
            var res =  _client.AddJob(jobData).ResponseAsync.Result;
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            var res = _client.AddJobs(jobDataSet).ResponseAsync.Result;
        }
    }
}