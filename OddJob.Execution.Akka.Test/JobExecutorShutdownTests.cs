using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace OddJob.Execution.Akka.Test
{
    public class JobExecutorShutdownTests
    {
        [Fact]
        public void JobExecutorShell_Will_Shutdown()
        {
            //Warning: this test is a bit racy, due to the nature of JobExecutor and the scheduler.
            //Lowering the timeouts may cause false failures on the test, as the timer may fire before the shutdown is even called.
            var jobStore = new InMemoryTestStore();
            jobStore.AddJob((ShellShutdownMockJob m) => m.DoThing(0), null, null, "test");
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())));
            executor.StartJobQueue("test", 5, 3);
            
            executor.ShutDownQueue("test");
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(ShellShutdownMockJob.MyCounter.ContainsKey(0) == false);
        }

        [Fact]
        public void JobExecutorShell_Will_Shutdown_Queues_on_Dispose()
        {
            //Warning: this test is a bit racy, due to the nature of JobExecutor and the scheduler.
            //Lowering the timeouts may cause false failures on the test, as the timer may fire before the shutdown is even called.
            var jobStore = new InMemoryTestStore();
            jobStore.AddJob((ShellShutdownMockJob m) => m.DoThing(1), null, null, "test");
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())));
            
            executor.StartJobQueue("test", 5, 3);
            
            
            executor.Dispose();
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(ShellShutdownMockJob.MyCounter.ContainsKey(1)==false);
        }
    }

    public class ShellShutdownMockJob
    {
        public static ConcurrentDictionary<int,int> MyCounter= new ConcurrentDictionary<int, int>();
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(derp, 1, ((k, v) => v + 1));
        }
    }
}