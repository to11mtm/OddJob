using System;
using System.Threading;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using Xunit;


namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class JobExecutorShellTests
    {
        [Fact]
        public void JobShell_Can_Start()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell<JobQueueLayerActor, JobWorkerActor, JobQueueCoordinator>(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new MockJobSuccessExecutor()),
                ()=> new JobQueueCoordinator(), null);
            executor.StartJobQueue(queueName,5,1);
        }

        [Fact]
        public void JobExecutorShell_Will_Execute_Jobs()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell<JobQueueLayerActor, JobWorkerActor, JobQueueCoordinator>(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), null);
            executor.StartJobQueue(queueName,5,1,1);
            jobStore.AddJob((ShellMockJob m) => m.DoThing(0),null,null,queueName);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.Equal(1,ShellMockJob.MyCounter);
        }

        
    }
}