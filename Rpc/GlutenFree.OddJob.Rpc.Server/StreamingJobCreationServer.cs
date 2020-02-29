using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Server.Hubs;
using OddJob.Rpc;
using OddJob.RpcServer;

namespace GlutenFree.OddJob.Rpc.Server
{
    public class
        StreamingJobCreationServer<TCacheType> : BaseStreamingJobCreationServer<
            StandardHubConnectionTracker<Guid, TCacheType>, TCacheType>
        where TCacheType : ITimedCache<Guid>, new()
    {
        public StreamingJobCreationServer(
            StreamingRPCServerAbstraction<
                StandardHubConnectionTracker<Guid, TCacheType>,
                TCacheType, Guid> abstraction) : base(abstraction)
        {
        }
    }

    public abstract class
        BaseStreamingJobCreationServer<TKeyCacheStore, TCacheStore> :
            BaseStreamingJobCreationServer
        where TCacheStore : ITimedCache<Guid>
        where TKeyCacheStore : IKeyedTimedCacheStore<TCacheStore, Guid>
    {

        public ConcurrentDictionary<string, IGroup> GroupSet { get; } =
            new ConcurrentDictionary<string, IGroup>();

        private StreamingRPCServerAbstraction<TKeyCacheStore, TCacheStore, Guid>
            _serverAbstraction;

        protected BaseStreamingJobCreationServer(
            StreamingRPCServerAbstraction<TKeyCacheStore, TCacheStore, Guid>
                serverAbstraction)
        {
            _serverAbstraction = serverAbstraction;
        }


        public override async Task CreateJob(SerializableOddJob jobData)
        {
            try
            {
                await _serverAbstraction.AddJobAndNotifySubscribers(jobData,
                    GroupFuncImpl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        private Task GroupFuncImpl(Guid[] guids, SerializableOddJob job)
        {

            Group.RawGroupRepository.TryGet(job.QueueName,
                out IGroup _oGroup);
            var broadCast =
                _oGroup != null ? BroadcastTo(_oGroup, guids) : null;
            var payload = new StreamingJobRequest()
            {
                JobId = job.JobId, QueueName = job.QueueName,
                MethodName = job.MethodName
            };
            broadCast?.JobCreated(payload);
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

        public override async Task CreateJobs(
            IEnumerable<SerializableOddJob> jobDataSet)
        {
           await _serverAbstraction.AddJobsAndNotifySubscribers(jobDataSet.ToList(),
                GroupFuncImpl);
        }
    }
}