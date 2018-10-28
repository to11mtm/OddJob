﻿using System;
using System.Collections.Concurrent;
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
            //Warning: this test is a bit racy, due to the nature of JobExecutor and the scheduler.
            //Lowering the timeouts may cause false failures on the test, as the timer may fire before the shutdown is even called.
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), null);
            executor.StartJobQueue("test", 5,5,5);
            jobStore.AddJob((ShellShutdownMockJob1 m) => m.DoThing(0), null, null, "test");
            executor.ShutDownQueue("test");
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(ShellShutdownMockJob1.MyCounter.ContainsKey(0) == false);
        }

        [Fact]
        public void JobExecutorShell_Will_Shutdown_Queues_on_Dispose()
        {
            //Warning: this test is a bit racy, due to the nature of JobExecutor and the scheduler.
            //Lowering the timeouts may cause false failures on the test, as the timer may fire before the shutdown is even called.
            var jobStore = new InMemoryTestStore();

            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), null);
            
            executor.StartJobQueue("test", 5, 1,1);
            jobStore.AddJob((ShellShutdownMockJob2 m) => m.DoThing(1), null, null, "test");

            executor.Dispose();

            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.False(executor.coordinatorPool.ContainsKey("test"));
            Xunit.Assert.True(ShellShutdownMockJob2.MyCounter.ContainsKey(1)==false);
        }
    }

    public class ShellShutdownMockJob1
    {
        internal static ConcurrentDictionary<int, int> MyCounter = new ConcurrentDictionary<int, int>();
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(derp, 1, ((k, v) => v + 1));
        }
    }

    public class ShellShutdownMockJob2
    {
        internal static ConcurrentDictionary<int,int> MyCounter= new ConcurrentDictionary<int, int>();
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(derp, 1, ((k, v) => v + 1));
        }
    }
}