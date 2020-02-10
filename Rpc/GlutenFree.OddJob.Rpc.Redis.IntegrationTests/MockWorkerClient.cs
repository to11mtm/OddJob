using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Client;
using OddJob.Rpc;
using OddJob.Rpc.Client;
using OddJob.RpcServer;

namespace Oddjob.Rpc.Redis.IntegrationTests
{
    public class MockWorkerClient : IJobHubReceiver
    {
        private GRPCChannelPool _pool;
        private RpcClientConfiguration _conf;

        private IJobHub client;

        public MockWorkerClient(GRPCChannelPool pool, RpcClientConfiguration conf)
        {
            _conf = conf;
            _pool = pool;
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
            count.AddOrUpdate(jobData.MethodName, mn => 1, (mn, v) => v + 1);
        }
        
        public static ConcurrentDictionary<string,int> count = new ConcurrentDictionary<string, int>();

        public async Task SendCreatedToServer(string mn, string qn)
        {
            try
            {

                await client.CreateJob(new SerializableOddJob() {MethodName = mn, QueueName = qn});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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