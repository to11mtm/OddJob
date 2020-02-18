using System;
using System.Threading;
using Akka.DI.SimpleInjector;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Integration.SimpleInjector;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class DependencyInjectedJobExecutorShellTests : AkkaExecutionTest
    {
        static DependencyInjectedJobExecutorShellTests()
        {
            AkkaTestUnitTestTableHelper.EnsureTablesExist();
        }
        
        [Fact]
        public void JobShell_Can_Start()
        {

            var queueName = QueueNameHelper.CreateQueueName();
            var container = new SimpleInjector.Container();
            container.Register<IContainerFactory, SimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver,DefaultJobAdderQueueTableResolver>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString));
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
            container.Register<IContainerFactory, SimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(() => new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString));
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
            container.Register<IContainerFactory, SimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<IJobQueueManager, SQLiteJobQueueManager>();
            container.Register<IJobQueueAdder, SQLiteJobQueueAdder>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString));
            container.Register<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
            container.Register<IJobExecutor, DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            container.Register<JobQueueCoordinator,OverriddenJobCoordinator>();
            container.Verify();
            var jobStore = (IJobQueueAdder)container.GetInstance(typeof(IJobQueueAdder));
            var executor = new DependencyInjectedJobExecutorShell(
                (system) => new SimpleInjectorDependencyResolver(container, system),
                null);
            /*var executor = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(jobStore),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())), null);*/
            jobStore.AddJob((DIShellMockJob m) => m.DoThing(nameof(DI_Semantics_Allow_Overridden_Coordinator), 0), null, null, queueName);
            executor.StartJobQueue(queueName, 5, 3, 1);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(
                DIShellMockJob.MyCounter.ContainsKey(nameof(DI_Semantics_Allow_Overridden_Coordinator)));
            Xunit.Assert.Equal(1, OverriddenJobCoordinator.Succeeded);
        }

    }
}