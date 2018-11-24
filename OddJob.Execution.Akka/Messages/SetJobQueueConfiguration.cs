using System;
using System.Linq.Expressions;
using Akka.Actor;

namespace GlutenFree.OddJob.Execution.Akka.Messages
{
    public class SetJobQueueConfiguration
    {
        public string QueueName { get; protected set; }
        public int NumWorkers { get; protected set; }
        public int PulseDelayInSeconds { get; protected set; }
        public int FirstPulseDelayInSeconds { get; protected set; }
        public Props WorkerProps { get; protected set; }
        public Props QueueProps { get; protected set; }
        public bool AggressiveSweep { get; protected set; }
        public Expression<Func<JobLockData, object>> PriorityExpression { get; protected set; }
        public SetJobQueueConfiguration(Props workerProps, Props queueProps, string queueName, int numWorkers,
            int pulseDelayInSeconds, int firstPulseDelayInSeconds = 5, Expression<Func<JobLockData, object>> priorityExpression = null, bool aggressiveSweep=false)
        {
            QueueName = queueName;
            NumWorkers = numWorkers;
            PulseDelayInSeconds = pulseDelayInSeconds;
            FirstPulseDelayInSeconds = firstPulseDelayInSeconds;
            WorkerProps = workerProps;
            QueueProps = queueProps;
            PriorityExpression = priorityExpression ?? ((JobLockData p)=> p.MostRecentDate);
            AggressiveSweep = aggressiveSweep;
        }
    }
}