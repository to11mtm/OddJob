using System;
using System.Collections.Generic;

namespace OddJob
{
    public interface IJobQueueManager
    {
        void MarkJobSuccess(Guid jobGuid);
        void MarkJobFailed(Guid jobGuid);
        IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames);
        void MarkJobInProgress(Guid jobId);
        void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt);
    }
}