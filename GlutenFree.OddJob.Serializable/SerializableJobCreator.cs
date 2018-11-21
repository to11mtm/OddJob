using System;
using System.Linq.Expressions;

namespace GlutenFree.OddJob.Serializable
{
    public static class SerializableJobCreator
    {
        public static SerializableOddJob CreateJobDefiniton<T>(Expression<Action<T>> jobExpression, RetryParameters retryParameters=null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            return new SerializableOddJob(JobCreator.Create(jobExpression),retryParameters,executionTime, queueName);
        }
    }
}