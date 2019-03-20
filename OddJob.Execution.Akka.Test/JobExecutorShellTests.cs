using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using Akka.DI.SimpleInjector;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;
using Xunit;
using Container = SimpleInjector.Container;


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

        public void Relase(object usedInstance)
        {
        }
    }

    public class OverriddenJobCoordinator : JobQueueCoordinator
    {
        protected override void OnJobSuccess(JobSuceeded msg)
        {
            Succeeded = Succeeded + 1;
        }

        public static int Succeeded;
    }

    public class DependencyInjectedJobExecutorShellTests : AkkaExecutionTest
    {
        
        [Fact]
        public void JobShell_Can_Start()
        {

            var queueName = QueueNameHelper.CreateQueueName();
            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver,DefaultJobAdderQueueTableResolver>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString));
            container.Register<IJobQueueManager,SQLiteJobQueueManager>();
            container.Register<IJobQueueAdder, SQLiteJobQueueAdder>();
            container.Register<IJobExecutor, MockJobSuccessExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            //container.Verify();
            // HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
            //() => new JobWorkerActor(new MockJobSuccessExecutor()), null);
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            executor.StartJobQueue(queueName, 5, 1);
        }


        [Fact]
        public void JobExecutorShell_Will_Execute_Jobs()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(() => new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString));
            container.Register<IJobQueueManager, SQLiteJobQueueManager>();
            container.Register<IJobQueueAdder, SQLiteJobQueueAdder>();
            container.Register<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
            container.Register<IJobExecutor,DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            container.Register<JobQueueCoordinator>();
            var jobStore = (IJobQueueAdder)container.GetInstance(typeof(IJobQueueAdder));
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            container.Verify();
            /*var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())), null);*/
            executor.StartJobQueue(queueName, 5, 1,1);
            jobStore.AddJob((DIShellMockJob m) => m.DoThing(nameof(JobExecutorShell_Will_Execute_Jobs),1), null, null, queueName);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.True(DIShellMockJob.MyCounter.ContainsKey(nameof(JobExecutorShell_Will_Execute_Jobs)));
        }

        [Fact]
        public void DI_Semantics_Allow_Overridden_Coordinator()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<IJobQueueManager, SQLiteJobQueueManager>();
            container.Register<IJobQueueAdder, SQLiteJobQueueAdder>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString));
            container.Register<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
            container.Register<IJobExecutor, DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            container.Register<JobQueueCoordinator,OverriddenJobCoordinator>();
            var jobStore = (IJobQueueAdder)container.GetInstance(typeof(IJobQueueAdder));
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            /*var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())), null);*/
            jobStore.AddJob((DIShellMockJob m) => m.DoThing(nameof(DI_Semantics_Allow_Overridden_Coordinator), 0), null, null, queueName);
            executor.StartJobQueue(queueName, 5, 1, 1);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(8));
            Xunit.Assert.True(
                DIShellMockJob.MyCounter.ContainsKey(nameof(DI_Semantics_Allow_Overridden_Coordinator)));
            Xunit.Assert.Equal(1, OverriddenJobCoordinator.Succeeded);
        }

    }

    public class DIShellMockJob
    {
        public static ConcurrentDictionary<string,int> MyCounter= new ConcurrentDictionary<string,int>();
        public void DoThing(string testName, int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(testName, (a) => 1, (a, b) => b + 1);
        }
    }

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