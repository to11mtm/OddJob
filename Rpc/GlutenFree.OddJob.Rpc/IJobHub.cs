using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion;
using MessagePack;

namespace OddJob.RpcServer
{
    public interface IJobHub : IStreamingHub<IJobHub, IJobHubReceiver>
    {
        Task CreateJob(SerializableOddJob jobData);
        Task JoinMonitoringAsync(string queueName, DateTime expiresAt);

        Task LeaveMonitoringAsync(string queueName);
        Task CreateJobs(IEnumerable<SerializableOddJob> jobDataSet);
    }
}