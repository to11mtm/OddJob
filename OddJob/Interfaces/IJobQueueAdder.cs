using System;
using System.Linq.Expressions;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobQueueResultWriter
    {
        void WriteJobQueueResult(Guid jobGuid, IOddJobResult result);
    }
    public interface IJobQueuePurger
    {
        void PurgeQueue(string name, string stateToPurge, DateTime purgeOlderThan);
    }
    public interface IJobQueueAdder
    {
        /// <summary>
        /// Adds a Job to the Queue.
        /// </summary>
        /// <typeparam name="TJob">The Type being used for the Job being executed.</typeparam>
        /// <param name="jobExpression">The Job Expression. Should be a single method call with parameters.</param>
        /// <param name="retryParameters">Retry Parameters to use for the job</param>
        /// <param name="executionTime">A time in the future to execute the job.</param>
        /// <param name="queueName">The Queue to place the job in.</param>
        /// <returns>GUID for the added Job</returns>
        Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters=null, DateTimeOffset? executionTime = null, string queueName = "default");
    }
}