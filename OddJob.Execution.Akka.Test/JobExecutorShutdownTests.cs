using System;
using System.Threading;
using GlutenFree.OddJob.Execution.BaseTests;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    [CollectionDefinition(name:"Shutdown", DisableParallelization = true)]
    public class JobExecutorShutdownTests
    {
        [Fact]
        public void JobExecutorShell_Will_Shutdown()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            //Warning: this test is a bit racy, due to the nature of JobExecutor and the scheduler.
            //Lowering the timeouts may cause false failures on the test, as the timer may fire before the shutdown is even called.
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell<JobQueueLayerActor, JobWorkerActor, JobQueueCoordinator>(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), null);
            jobStore.AddJob((ShellShutdownMockJob1 m) => m.DoThing(0), null, null, queueName);
            executor.StartJobQueue(queueName, 5,5,5);
            executor.ShutDownQueue(queueName);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(ShellShutdownMockJob1.MyCounter.ContainsKey(0) == false);
        }
    }
}