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
            get { return _dependencyResolver.Create(typeof(JobWorkerActor)); }
        }

        protected override Props JobQueueProps
        {
            get { return _dependencyResolver.Create(typeof(JobQueueLayerActor)); }
        }
    }
}