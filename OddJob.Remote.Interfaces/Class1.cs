using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;

namespace OddJob.Remote.Interfaces
{
    public abstract class BaseRemoteJobQueueAdder : ISerializedJobQueueAdder
    {
        public BaseRemoteJobQueueAdder(ISerializedJobQueueAdder innerJobQueueAdder)
        {
            InnerJobQueueAdder = innerJobQueueAdder ?? throw new ArgumentNullException(nameof(innerJobQueueAdder));
        }

        protected ISerializedJobQueueAdder InnerJobQueueAdder { get; set; }

        public void AddJob(SerializableOddJob jobData)
        {
            InnerJobQueueAdder.AddJob(jobData);
        }

        public async Task AddJobAsync(SerializableOddJob jobData, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            throw new NotImplementedException();
        }

        public async Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}