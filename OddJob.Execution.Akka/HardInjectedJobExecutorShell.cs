using System;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;

namespace OddJob.Execution.Akka
{
    public class HardInjectedJobExecutorShell : BaseJobExecutorShell
    {
        public HardInjectedJobExecutorShell(Expression<Func<JobQueueLayerActor>> jobQueueFunc, Expression<Func<JobWorkerActor>> workerFunc, Func<IRequiresMessageQueue<ILoggerMessageQueueSemantics>> loggerTypeFactory) : base(loggerTypeFactory)
        {
            JobQueueProps = Props.Create(jobQueueFunc);
            WorkerProps = Props.Create(workerFunc);
        }
        protected override Props WorkerProps { get; }
        protected override Props JobQueueProps { get; }
    }
}