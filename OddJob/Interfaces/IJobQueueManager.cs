using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobQueueManager
    {
        void MarkJobSuccess(Guid jobGuid);
        void MarkJobFailed(Guid jobGuid);
        IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize, Expression<Func<JobLockData, object>> orderPredicate);
        void MarkJobInProgress(Guid jobId);
        void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt);
        IOddJobWithMetadata GetJob(Guid jobId);
    }
}