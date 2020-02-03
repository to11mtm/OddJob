using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Server.Hubs;

namespace OddJob.RpcServer
{
    public class
        StreamingJobCreationServer :
            StreamingHubBase<IJobHub, IJobHubReceiver>, IJobHub
    {
        public StreamingJobCreationServer(
            ISerializedJobQueueAdder jobQueueAdder)
        {
            _jobQueueAdder = jobQueueAdder;
        }
        public IGroup _group;
        Random r = new Random();
        private ISerializedJobQueueAdder _jobQueueAdder;

        public Task CreateJob(SerializableOddJob jobData)
        {
            //await Group.AddAsync($"adder-{jobData.QueueName}",
            //    new ClientId() {Id = ConnectionId});
            _jobQueueAdder.AddJob(jobData);

            NotifyQueueSubscribers(jobData);

            return Task.CompletedTask;
        }

        private void NotifyQueueSubscribers(SerializableOddJob jobData)
        {
            if (Group.RawGroupRepository.TryGet(jobData.QueueName,
                out IGroup broadGroup))
            {
                var nd = r.NextDouble();

                var vals = broadGroup.GetInMemoryStorage<StorageId>()
                    .AllValues;

                Guid[] broadcastlist = null;
                broadcastlist = vals.Select((id, i) => new {id, i}).Where(
                        vidx =>
                            (nd > 0.5) ? vidx.i % 2 > 0 : vidx.i % 2 == 0)
                    .Select(r => r.id.Id).ToArray();

                if (broadcastlist.Length < 4)
                {
                    broadcastlist = vals.Select(sid => sid.Id).ToArray();
                }

                BroadcastTo(broadGroup, broadcastlist).JobCreated(jobData);
            }
        }

        public async Task JoinMonitoringAsync(string queueName)
        {
            IInMemoryStorage<StorageId> sto;
            (_group,sto ) = await Group.AddAsync(queueName,
                new StorageId() {Id = ConnectionId});
        }

        public async Task LeaveMonitoringAsync(string queueName)
        {
            await _group.RemoveAsync(this.Context);
        }

        public Task CreateJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            _jobQueueAdder.AddJobs(jobDataSet);
            foreach (var job in jobDataSet)
            {
                NotifyQueueSubscribers(job);
            }

            return Task.CompletedTask;
        }
    }
}