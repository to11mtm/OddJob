using System;
using System.Linq.Expressions;

namespace OddJob
{
    public interface IJobQueueAdder
    {
        Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, string queueName = "default");
        Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, DateTimeOffset? executionTime, string queueName = "default");
    }
}