using System;
using System.Collections.Generic;
using System.Linq;
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
            OnJobAdded(jobData);
        }

        public async Task AddJobAsync(SerializableOddJob jobData, CancellationToken cancellationToken = default)
        {
            await InnerJobQueueAdder.AddJobAsync(jobData, cancellationToken);
            await OnJobAddedAsync(jobData, cancellationToken);
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            var set = jobDataSet.ToList();
            InnerJobQueueAdder.AddJobs(set);
            OnJobsAdded(set);
        }

        public async Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet, CancellationToken cancellationToken = default)
        {
            var set = jobDataSet.ToList();
            await InnerJobQueueAdder.AddJobsAsync(set, cancellationToken);
            await OnJobsAddedAsync(set, cancellationToken);
        }
        
        protected abstract void OnJobAdded(SerializableOddJob jobData);
        protected abstract Task OnJobAddedAsync(SerializableOddJob jobData, CancellationToken cancellationToken);
        
        protected abstract void OnJobsAdded(List<SerializableOddJob> set);
        protected abstract Task OnJobsAddedAsync(List<SerializableOddJob> set, CancellationToken cancellationToken);
    }
}