using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IRetryParameters
    {
        int MaxRetries { get; }
        TimeSpan MinRetryWait { get; }
        int RetryCount { get; }
        DateTime? LastAttempt { get; }
    }
}