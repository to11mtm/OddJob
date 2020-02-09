using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IOddJobWithMetadata : IOddJob
    {
        DateTimeOffset? ExecutionTime { get; }
        IRetryParameters RetryParameters { get; }
        string Queue { get; }
    }
}