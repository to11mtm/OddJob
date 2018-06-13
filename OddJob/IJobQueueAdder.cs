using System;
using System.Linq.Expressions;

namespace OddJob
{
    public interface IJobQueueAdder
    {
        Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters=null, DateTimeOffset? executionTime = null, string queueName = "default");
    }

    public class RetryParameters
    {
        public int MaxRetries { get; set; }
        public TimeSpan MinRetryWait { get; set; }

        public RetryParameters(int maxRetries, TimeSpan minRetryWait)
        {
            MaxRetries = maxRetries;
            MinRetryWait = minRetryWait;
        }
    }
}