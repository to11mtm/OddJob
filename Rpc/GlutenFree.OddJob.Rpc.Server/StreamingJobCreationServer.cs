using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Server.Hubs;
using OddJob.RpcServer;

namespace GlutenFree.OddJob.Rpc.Server
{
    public class StreamingJobCreationServer : BaseStreamingJobCreationServer<TimedCache<Guid>>
    {
        public StreamingJobCreationServer(ISerializedJobQueueAdder jobQueueAdder, StandardHubConnectionTracker<Guid> timeCache, StreamingJobCreationServerOptions options) : base(jobQueueAdder, timeCache, options)
        {
        }
    }

    public abstract class BaseStreamingJobCreationServer : StreamingHubBase<IJobHub, IJobHubReceiver>, IJobHub
    {
        public abstract Task CreateJob(SerializableOddJob jobData);
        public abstract Task JoinMonitoringAsync(string queueName, DateTime expiresAt);
        public abstract Task LeaveMonitoringAsync(string queueName);
        public abstract Task CreateJobs(IEnumerable<SerializableOddJob> jobDataSet);
    }
    public abstract class
        BaseStreamingJobCreationServer<TCacheStore> :
            BaseStreamingJobCreationServer
    where TCacheStore : ITimedCache<Guid>
    {
        public IKeyedTimedCacheStore<TCacheStore,Guid> LiveConnections { get; }
        public  ConcurrentDictionary<string, IGroup> GroupSet { get; }= new ConcurrentDictionary<string, IGroup>();
        protected BaseStreamingJobCreationServer(
            ISerializedJobQueueAdder jobQueueAdder, IKeyedTimedCacheStore<TCacheStore,Guid> timeCache, StreamingJobCreationServerOptions options)
        {
            _jobQueueAdder = jobQueueAdder;
            LiveConnections = timeCache;
            _options = options;
        }
        Random r = new Random();
        private ISerializedJobQueueAdder _jobQueueAdder;
        private StreamingJobCreationServerOptions _options;

        public override Task CreateJob(SerializableOddJob jobData)
        {
            //await Group.AddAsync($"adder-{jobData.QueueName}",
            //    new ClientId() {Id = ConnectionId});
            _jobQueueAdder.AddJob(jobData);

            NotifyQueueSubscribers(jobData);

            return Task.CompletedTask;
        }
        
        private void NotifyQueueSubscribers(SerializableOddJob jobData)
        {
            try
            {

            
            var connQ = LiveConnections.GetOrCreate(jobData.QueueName);
            var set = connQ.GetItems().ToList();
            Group.RawGroupRepository.TryGet(jobData.QueueName,
                out IGroup _group);
            if (set.Any() && _group != null)
            {
                var nd = r.NextDouble();


                List<Guid> broadcastlist = null;
                broadcastlist = set.Select((id, i) => new {id, i}).Where(
                        vidx =>
                            (nd > 0.5) ? vidx.i % 2 > 0 : vidx.i % 2 == 0)
                    .Select(rec => rec.id).Take(_options.MaxBroadcastToNodes).ToList();

                if (broadcastlist.Count < _options.MinBroadcastToNodes)
                {
                    var newSet = set.Where(r=> broadcastlist.Contains(r) == false).ToList();
                    broadcastlist.AddRange(newSet.Take(
                        _options.MinBroadcastToNodes - broadcastlist.Count));
                    
                }

                BroadcastTo(_group, broadcastlist.ToArray()).JobCreated(jobData);
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public override async Task JoinMonitoringAsync(string queueName, DateTime expiresAt)
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

        public override async Task LeaveMonitoringAsync(string queueName)
        {
            Group.RawGroupRepository.TryGet(queueName, out IGroup _group);
            if (_group != null)
            {
                await _group.RemoveAsync(this.Context);    
            }
            
        }

        public override Task CreateJobs(IEnumerable<SerializableOddJob> jobDataSet)
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