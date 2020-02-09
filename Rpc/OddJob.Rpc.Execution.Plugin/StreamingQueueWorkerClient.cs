using System;
using System.Threading.Tasks;
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