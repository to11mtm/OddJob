using System;
using System.Threading;
using Akka.DI.SimpleInjector;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using GlutenFree.OddJob.Interfaces;
using SimpleInjector;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class TestSimpleInjectorContainerFactory : IContainerFactory
    {
        private Container _container;

        public TestSimpleInjectorContainerFactory(Container container)
        {
            _container = container;
        }
        public object CreateInstance(Type typeToCreate)
        {
            return _container.GetInstance(typeToCreate);
        }
    }
    public class DependencyInjectedJobExecutorShellTests
    {
        
        [Fact]
        public void JobShell_Can_Start()
        {

            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<IJobQueueManager,InMemoryTestStore>();
            container.Register<IJobQueueAdder, InMemoryTestStore>();
            container.Register<IJobExecutor, MockJobSuccessExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            //container.Verify();
            // HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
            //() => new JobWorkerActor(new MockJobSuccessExecutor()), null);
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            executor.StartJobQueue("test", 5, 1);
        }

        [Fact]
        public void JobExecutorShell_Will_Execute_Jobs()
        {
            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<IJobQueueManager, InMemoryTestStore>();
            container.Register<IJobQueueAdder, InMemoryTestStore>();
            container.Register<IJobExecutor, DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            var jobStore = (InMemoryTestStore)container.GetInstance(typeof(InMemoryTestStore));
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            /*var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())), null);*/
            executor.StartJobQueue("test", 5, 1);
            jobStore.AddJob((DIShellMockJob m) => m.DoThing(0), null, null, "test");
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.Equal(1, DIShellMockJob.MyCounter);
        }


    }

    public class DIShellMockJob
    {
        public static int MyCounter = 0;
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter = MyCounter + 1;
        }
    }

    public class JobExecutorShellTests
    {
        [Fact]
        public void JobShell_Can_Start()
        {
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new MockJobSuccessExecutor()), null);
            executor.StartJobQueue("test",5,1);
        }

        [Fact]
        public void JobExecutorShell_Will_Execute_Jobs()
        {
            var jobStore = new InMemoryTestStore();
            var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),null);
            executor.StartJobQueue("test",5,3);
            jobStore.AddJob((ShellMockJob m) => m.DoThing(0),null,null,"test");
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.Equal(1,ShellMockJob.MyCounter);
        }

        
    }


    public class ShellMockJob
    {
        internal static int MyCounter = 0;
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter = MyCounter + 1;
        }
    }
}