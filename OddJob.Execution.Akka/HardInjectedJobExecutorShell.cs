using System;
using System.Linq.Expressions;
using Akka.Actor;

namespace OddJob.Execution.Akka
{
    public class HardInjectedJobExecutorShell : BaseJobExecutorShell
    {
        public HardInjectedJobExecutorShell(Expression<Func<JobQueueLayerActor>> jobQueueFunc, Expression<Func<JobWorkerActor>> workerFunc)
        {
            JobQueueProps = Props.Create(jobQueueFunc);
            WorkerProps = Props.Create(workerFunc);
        }
        protected override Props WorkerProps { get; }
        protected override Props JobQueueProps { get; }
    }
}