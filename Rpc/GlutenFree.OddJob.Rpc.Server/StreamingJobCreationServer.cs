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
    public class
        StreamingJobCreationServer : BaseStreamingJobCreationServer<StandardHubConnectionTracker<Guid>, TimedCache<Guid>>
    {
        public StreamingJobCreationServer(
            StreamingRPCServerAbstraction<StandardHubConnectionTracker<Guid>,TimedCache<Guid>,Guid> abstraction) : base(abstraction)
        {
        }
    }

    public abstract class
        BaseStreamingJobCreationServer :
            StreamingHubBase<IJobHub, IJobHubReceiver>, IJobHub
    {
        public abstract Task CreateJob(SerializableOddJob jobData);

        public abstract Task JoinMonitoringAsync(string queueName,
            DateTime expiresAt);

        public abstract Task LeaveMonitoringAsync(string queueName);

        public abstract Task CreateJobs(
            IEnumerable<SerializableOddJob> jobDataSet);
    }

    public class StreamingRPCServerAbstraction<TKeyCacheStore,TCacheStore,TKey>
    where TKeyCacheStore:IKeyedTimedCacheStore<TCacheStore,TKey>
    where TCacheStore:ITimedCache<TKey>
    {
        
        Random r = new Random();
        private ISerializedJobQueueAdder _jobQueueAdder;
        private StreamingJobCreationServerOptions _options;
        public TKeyCacheStore LiveConnections { get; }
        public StreamingRPCServerAbstraction(
            ISerializedJobQueueAdder jobQueueAdder,
            TKeyCacheStore timeCache,
            StreamingJobCreationServerOptions options)
        {
            
            _jobQueueAdder = jobQueueAdder;
            LiveConnections = timeCache;
            _options = options;
        }

        public  async Task AddJobAndNotifySubscribers(SerializableOddJob job, Func<TKey[], SerializableOddJob, Task> groupFunc)
        { 
            await _jobQueueAdder.AddJobAsync(job);
            await NotifyQueueSubscribers(job, groupFunc);
            
        }

        public void JoinMonitoring(TKey connectionId, string queueName,
            DateTime expiresAt)
        {
            
            var connQ =
                LiveConnections.GetOrCreate(queueName);
            connQ.Freshen(connectionId, expiresAt);
        }

        public void AddJobsAndNotifySubscribers(
            List<SerializableOddJob> jobDataSet,
            Func<TKey[], SerializableOddJob, Task> groupFunc)
        {
            _jobQueueAdder.AddJobs(jobDataSet);
            foreach (var job in jobDataSet)
            { 
                NotifyQueueSubscribers(job, groupFunc);
            }
        }
        private  async Task NotifyQueueSubscribers(SerializableOddJob jobData, Func<TKey[], SerializableOddJob, Task> groupFunc)
        {
            try
            {


                var connQ = LiveConnections.GetOrCreate(jobData.QueueName);
                var set = connQ.GetItems().ToList();
                if (set.Any())
                {
                    var nd = r.NextDouble();


                    List<TKey> broadcastlist = null;
                    broadcastlist = set.Select((id, i) => new {id, i}).Where(
                            vidx =>
                                (nd > 0.5) ? vidx.i % 2 > 0 : vidx.i % 2 == 0)
                        .Select(rec => rec.id)
                        .Take(_options.MaxBroadcastToNodes).ToList();

                    if (broadcastlist.Count < _options.MinBroadcastToNodes)
                    {
                        var newSet = set
                            .Where(r => broadcastlist.Contains(r) == false)
                            .ToList();
                        broadcastlist.AddRange(newSet.Take(
                            _options.MinBroadcastToNodes -
                            broadcastlist.Count));

                    }

                   await groupFunc(broadcastlist.ToArray(), jobData);

                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    public abstract class
        BaseStreamingJobCreationServer<TKeyCacheStore,TCacheStore> :
            BaseStreamingJobCreationServer
        where TCacheStore : ITimedCache<Guid> where TKeyCacheStore : IKeyedTimedCacheStore<TCacheStore, Guid>
    {

        public ConcurrentDictionary<string, IGroup> GroupSet { get; } =
            new ConcurrentDictionary<string, IGroup>();

        private StreamingRPCServerAbstraction<TKeyCacheStore,TCacheStore, Guid>
            _serverAbstraction;

        protected BaseStreamingJobCreationServer(
            StreamingRPCServerAbstraction<TKeyCacheStore, TCacheStore,Guid> serverAbstraction)
        {
            _serverAbstraction = serverAbstraction;
        }


        public override async Task CreateJob(SerializableOddJob jobData)
        {
            try
            {

            
            await _serverAbstraction.AddJobAndNotifySubscribers(jobData, GroupFuncImpl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        private Task  GroupFuncImpl(Guid[] guids, SerializableOddJob job)
        {
            
                Group.RawGroupRepository.TryGet(job.QueueName,
                    out IGroup _oGroup);
                var broadCast =  _oGroup != null ? BroadcastTo(_oGroup, guids) : null;
                broadCast?.JobCreated(job);
                return Task.CompletedTask;
        }

        public override async Task JoinMonitoringAsync(string queueName,
            DateTime expiresAt)
        {
            var grp =
                GroupSet.GetOrAdd(queueName,
                    qn => Group.AddAsync(queueName).Result);

            _serverAbstraction.JoinMonitoring(ConnectionId, queueName,
                expiresAt);
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

        public override Task CreateJobs(
            IEnumerable<SerializableOddJob> jobDataSet)
        { 
            _serverAbstraction.AddJobsAndNotifySubscribers(jobDataSet.ToList(),
                GroupFuncImpl);
return Task.CompletedTask;
        }
    }
}