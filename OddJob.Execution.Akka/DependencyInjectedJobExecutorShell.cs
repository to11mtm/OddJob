using System;
using Akka.Actor;
using Akka.DI.Core;

namespace OddJob.Execution.Akka
{
    public class DependencyInjectedJobExecutorShell : BaseJobExecutorShell
    {
        IDependencyResolver _dependencyResolver;
        protected DependencyInjectedJobExecutorShell(Func<ActorSystem, IDependencyResolver> dependencyResolverCreator)
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