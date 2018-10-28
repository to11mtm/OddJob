using System;
using Akka.Actor;
using Akka.Dispatch;
using Akka.DI.Core;
using Akka.Event;

namespace GlutenFree.OddJob.Execution.Akka
{

    public class DependencyInjectedJobExecutorShell : BaseJobExecutorShell
    {
        IDependencyResolver _dependencyResolver;
        public DependencyInjectedJobExecutorShell(Func<ActorSystem, IDependencyResolver> dependencyResolverCreator, Func<IRequiresMessageQueue<ILoggerMessageQueueSemantics>> loggerTypeFactory) :base(loggerTypeFactory)
        {
            _dependencyResolver = dependencyResolverCreator(_actorSystem);
        }

        
        protected override Props WorkerProps
        {
            get { return _dependencyResolver.Create(WorkerType); }
        }

        /// <summary>
        /// Override this to have a custom worker type.
        /// </summary>
        protected virtual Type WorkerType
        {
            get { return (typeof(JobWorkerActor)); }
        }

        /// <summary>
        /// Override this to provide a custom Queue Layer type.
        /// </summary>
        protected virtual Type QueueLayerType
        {
            get { return (typeof(JobQueueLayerActor)); }
        }

        protected override Props JobQueueProps
        {
            get { return _dependencyResolver.Create(QueueLayerType); }
        }

        protected override Props CoordinatorProps
        {
            get { return _dependencyResolver.Create(CoordinatorType); }
        }

        protected virtual Type CoordinatorType
        {
            get { return (typeof(JobQueueCoordinator)); }
        }


    }
}