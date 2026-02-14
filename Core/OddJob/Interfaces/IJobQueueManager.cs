using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobQueueManager
    {
        void MarkJobSuccess(Guid jobGuid);
        void MarkJobFailed(Guid jobGuid);
        IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize, Expression<Func<JobLockData, object>> orderPredicate);
        void MarkJobInProgress(Guid jobId);
        void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt);
        IOddJobWithMetadata GetJob(Guid jobId, bool needLock = true, bool requireValidStatus = true);
        
        Task MarkJobSuccessAsync(Guid jobGuid, CancellationToken cancellationToken= default);
        Task MarkJobFailedAsync(Guid jobGuid, CancellationToken cancellationToken = default);
        Task<IEnumerable<IOddJobWithMetadata>> GetJobsAsync(string[] queueNames, int fetchSize, Expression<Func<JobLockData, object>> orderPredicate, CancellationToken cancellationToken = default);
        Task MarkJobInProgressAsync(Guid jobId, CancellationToken cancellationToken = default);
        Task MarkJobInRetryAndIncrementAsync(Guid jobId, DateTime lastAttempt, CancellationToken cancellationToken = default);
        Task<IOddJobWithMetadata> GetJobAsync(Guid jobId, bool needLock = true,bool requireValidStatus=true, CancellationToken cancellationToken = default);
    }
}