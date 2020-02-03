using System;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Client;
using OddJob.Rpc.Client;
using OddJob.RpcServer;

namespace OddJob.Rpc.Execution.Plugin
{
    public class StreamingQueueWorkerClient : IJobHubReceiver
    {
        private GRPCChannelPool _pool;
        private RpcClientConfiguration _conf;

        private IJobHub client;
        private IActorRef _parent;

        public StreamingQueueWorkerClient(GRPCChannelPool pool, RpcClientConfiguration conf, IActorRef parent)
        {
            _conf = conf;
            _pool = pool;
            _parent = parent;
            Create();
        }
        public void Create()
        {
            client = StreamingHubClient.Connect<IJobHub, IJobHubReceiver>(
                _pool.GetChannel(_conf), this,
                serializerOptions: UnregisteredSerializerOptions.Instance);
        }

        public void JobCreated(SerializableOddJob jobData)
        {
            
            _parent.Tell(new GetSpecificJob(jobData.JobId, jobData.QueueName));
        }

        public void Join(string queueName)
        {
            client.JoinMonitoringAsync(queueName);
        }

        public void Stop(string queueName)
        {
            client.LeaveMonitoringAsync(queueName);
            client.DisposeAsync().Wait();
        }
    }
}