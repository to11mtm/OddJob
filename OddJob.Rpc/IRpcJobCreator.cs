using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion;

namespace OddJob.RpcServer
{
    public interface IRpcJobCreator : IService<IRpcJobCreator>
    {
        UnaryResult<Guid?> AddJob(SerializableOddJob jobData);
        UnaryResult<List<Guid>> AddJobs(IEnumerable<SerializableOddJob> jobs);
    }
    public interface IJobHubReceiver
    {
        void JobCreated(SerializableOddJob jobData);
    }

    public interface IJobHub : IStreamingHub<IJobHub, IJobHubReceiver>
    {
        Task CreateJob(SerializableOddJob jobData);
        Task JoinMonitoringAsync(string queueName);

        Task LeaveMonitoringAsync(string queueName);
        Task CreateJobs(IEnumerable<SerializableOddJob> jobDataSet);
    }
}