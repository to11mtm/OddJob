using System;
using System.Linq;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class HardInjectedJobExecutorShell<TJobLayerActor,TJobWorkerActor,TJobCoordinator> : BaseJobExecutorShell
    where TJobLayerActor: JobQueueLayerActor
    where TJobWorkerActor : JobWorkerActor
    where TJobCoordinator : JobQueueCoordinator
    {
        public HardInjectedJobExecutorShell(Expression<Func<TJobLayerActor>> jobQueueFunc,
            Expression<Func<TJobWorkerActor>> workerFunc,
            Expression<Func<TJobCoordinator>> coordinatorFunc,
            IExecutionEngineLoggerConfig loggerConfig) : base(loggerConfig)
        {
            JobQueueProps = Props.Create(jobQueueFunc);
            WorkerProps = Props.Create(workerFunc);
            CoordinatorProps = Props.Create(coordinatorFunc);
        }


        protected override Props WorkerProps { get; }
        protected override Props JobQueueProps { get; }
        protected override Props CoordinatorProps { get; }
    }

    
}