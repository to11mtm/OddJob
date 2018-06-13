using System;
using System.Linq.Expressions;

namespace OddJob
{
    public interface IJobQueueAdder
    {
        Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters=null, DateTimeOffset? executionTime = null, string queueName = "default");
    }
}