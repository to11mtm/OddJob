using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using MagicOnion.Server.Hubs;
using OddJob.RpcServer;

namespace GlutenFree.OddJob.Rpc.Server
{
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
}