using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Client;
using OddJob.RpcServer;

namespace OddJob.Rpc.Client
{
    public class StreamingQueueClient : IJobHubReceiver, ISerializedJobQueueAdder
    {
        private GRPCChannelPool _pool;
        private RpcClientConfiguration _conf;

        private IJobHub client;
        public StreamingQueueClient(GRPCChannelPool pool, RpcClientConfiguration conf)
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
            
        }

        public void AddJob(SerializableOddJob jobData)
        {
            client.CreateJob(jobData).Wait();
        }

        public async Task AddJobAsync(SerializableOddJob jobData,
            CancellationToken cancellationToken = default)
        {
            await client.CreateJob(jobData);
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            client.CreateJobs(jobDataSet).Wait();
        }

        public   Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet,
            CancellationToken cancellationToken = default)
        {
            return client.CreateJobs(jobDataSet);
        }



        public async Task CloseAsync()
        {
            await client.DisposeAsync();
        }
    }
}