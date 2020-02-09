using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.XPath;
using GlutenFree.OddJob.Serializable;
using MagicOnion;
using MagicOnion.Server.Hubs;
using MessagePack;

namespace OddJob.RpcServer
{

    public class StreamingJobCreationServer : BaseStreamingJobCreationServer<TimedCache<Guid>>
    {
        public StreamingJobCreationServer(ISerializedJobQueueAdder jobQueueAdder, StandardHubConnectionTracker<Guid> timeCache) : base(jobQueueAdder, timeCache)
        {
        }
    }
    public abstract class
        BaseStreamingJobCreationServer<TCacheStore> :
            StreamingHubBase<IJobHub, IJobHubReceiver>, IJobHub
    where TCacheStore : ITimedCache<Guid>
    {
        public IKeyedTimedCacheStore<TCacheStore,Guid> LiveConnections { get; }
        public  ConcurrentDictionary<string, IGroup> GroupSet { get; }= new ConcurrentDictionary<string, IGroup>();
        protected BaseStreamingJobCreationServer(
            ISerializedJobQueueAdder jobQueueAdder, IKeyedTimedCacheStore<TCacheStore,Guid> timeCache)
        {
            _jobQueueAdder = jobQueueAdder;
            LiveConnections = timeCache;
        }
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
            
            var connQ = LiveConnections.GetOrCreate(jobData.QueueName);
            var set = connQ.GetItems().ToList();
            Group.RawGroupRepository.TryGet(jobData.QueueName,
                out IGroup _group);
            if (set.Any() && _group != null)
            {
                var nd = r.NextDouble();


                Guid[] broadcastlist = null;
                broadcastlist = set.Select((id, i) => new {id, i}).Where(
                        vidx =>
                            (nd > 0.5) ? vidx.i % 2 > 0 : vidx.i % 2 == 0)
                    .Select(rec => rec.id).ToArray();

                if (broadcastlist.Length < 4)
                {
                    broadcastlist = set.ToArray();
                }

                BroadcastTo(_group, broadcastlist).JobCreated(jobData);
            }
        }
        
        public async Task JoinMonitoringAsync(string queueName, DateTime expiresAt)
        {
            var grp =
                GroupSet.GetOrAdd(queueName,
                     qn => Group.AddAsync(queueName).Result);

            var connQ =
                LiveConnections.GetOrCreate(queueName);
            connQ.Freshen(ConnectionId, expiresAt);
            try
            {
                await OnJoinMonitoringAsync(queueName, expiresAt);
            }
            catch
            {
            }
        }
        
        protected virtual async Task OnJoinMonitoringAsync(
            string queueName, DateTime expiresAt)
        {

        }

        public async Task LeaveMonitoringAsync(string queueName)
        {
            Group.RawGroupRepository.TryGet(queueName, out IGroup _group);
            if (_group != null)
            {
                await _group.RemoveAsync(this.Context);    
            }
            
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