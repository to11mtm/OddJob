using System;
using System.Threading.Tasks;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Serializable;
using Grpc.Core;
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
        private IActorRef _pluginRef;

        public StreamingQueueWorkerClient(GRPCChannelPool pool, RpcClientConfiguration conf, IActorRef parent, IActorRef pluginRef)
        {
            _conf = conf;
            _pool = pool;
            _parent = parent;
            _pluginRef = pluginRef;
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
            var jobID = jobData.JobId;
            var qn = jobData.QueueName;
            _parent.Tell(new GetSpecificJob(jobID, qn));
        }

        public async Task Join(string queueName, DateTime expiresAt)
        {
            await client.JoinMonitoringAsync(queueName, expiresAt);
        }

        public async Task Stop(string queueName)
        {
            await client.LeaveMonitoringAsync(queueName);
            await client.DisposeAsync();
        }

        public async Task Refresh(string queueName, DateTime expiresAt)
        {
            await client.JoinMonitoringAsync(queueName, expiresAt);
        }
    }
}