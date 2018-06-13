using System;
using System.Collections.Generic;

namespace OddJob
{
    public interface IJobQueueManager
    {
        void MarkJobSuccess(Guid jobGuid);
        void MarkJobFailed(Guid jobGuid);
        IEnumerable<IOddJob> GetJobs(string[] queueNames);
        void MarkJobInProgress(Guid jobId);
    }
}