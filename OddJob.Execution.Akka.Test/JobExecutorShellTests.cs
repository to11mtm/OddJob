using System;
using System.Threading;
using Xunit;

namespace OddJob.Execution.Akka.Test
{
    public class JobExecutorShellTests
    {
        [Fact]
        public void JobShell_Can_Start()
        {
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new MockJobSuccessExecutor()));
            executor.StartJobQueue("test",5,1);
        }

        [Fact]
        public void JobExecutorShell_Will_Execute_Jobs()
        {
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())));
            executor.StartJobQueue("test",5,3);
            jobStore.AddJob((ShellMockJob m) => m.DoThing(0),null,null,"test");
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.Equal(1,ShellMockJob.MyCounter);
        }

        
    }


    public class ShellMockJob
    {
        public static int MyCounter = 0;
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter = MyCounter + 1;
        }
    }
}