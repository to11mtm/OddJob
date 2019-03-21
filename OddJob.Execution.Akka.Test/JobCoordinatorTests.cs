using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using Akka.TestKit.Xunit2;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.Common;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public static class UnitTestTableHelper
    {
        internal static readonly string connString = "FullUri=file::memory:?cache=shared";
        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        public static bool TablesCreated = false;
        static UnitTestTableHelper()
        {
            heldConnection = new SQLiteConnection(connString);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void EnsureTablesExist()
        {
            if (TablesCreated)
            {
                return;
                ;
            }
            if (heldConnection.State != ConnectionState.Open)
            {
                heldConnection.Open();
            }

            using (var db = new SQLiteConnection(connString))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueTableName);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueParamTableName);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                        SqlDbJobQueueDefaultTableConfiguration.DefaultJobMethodGenericParamTableName);
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SQLiteDbJobTableHelper.JobQueueParamTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SQLiteDbJobTableHelper.JobTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        SQLiteDbJobTableHelper.JobQueueJobMethodGenericParamTableCreateScript(
                            new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }

            TablesCreated = true;




        }
    }

    public class ShutdownFixture
    {

    }

    [CollectionDefinition("Require Synchronous Run", DisableParallelization = true)]
    public class SensitiveShutdownCollection : ICollectionFixture<ShutdownFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    public static class QueueNameHelper
    {

        public static string CreateQueueName()
        {
            return Guid.NewGuid().ToString();
        }
    }

    public class AkkaExecutionTest : TestKit
    {


        public AkkaExecutionTest()
        {
            UnitTestTableHelper.EnsureTablesExist();
        }

        public static IJobQueueManager GetJobQueueManager
        {
            get
            {
                return new SQLiteJobQueueManager(
                    new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString),
                    new SqlDbJobQueueDefaultTableConfiguration(), new NullOnMissingTypeJobTypeResolver());
            }
        }

        public static IJobQueueAdder GetJobQueueAdder
        {
            get
            {
                return new SQLiteJobQueueAdder(new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString),
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }
    }

    public class CountingOnJobQueueSaturatedCoordinator : JobQueueCoordinator
    {
        public static ConcurrentDictionary<string,int> pulseCount = new ConcurrentDictionary<string, int>();
        protected override void OnJobQueueSaturated(DateTime saturationTime, int saturationMissedPulseCount, long queueLifeSaturationPulseCount)
        {
            pulseCount.AddOrUpdate(QueueName, (qn) => 1, (qn, i) => i + 1);
        }
    }

    public class DelayJob
    {
        public static ConcurrentDictionary<string,int> MsgCounter = new ConcurrentDictionary<string, int>();
        public void DoDelay(string msg)
        {
            MsgCounter.AddOrUpdate(msg, (m) => 1, (m, i) => i + 1);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
        }
    }
    [Collection("Require Synchronous Run")]
    public class JobCoordinatorTests : AkkaExecutionTest
    {
        [Fact]
        public void JobCoordinator_Can_Shutdown()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var workerCount = 5;
            var jobStore = AkkaExecutionTest.GetJobQueueManager;
            var executor = new MockJobSuccessExecutor();
            var queueLayerProps = Props.Create(() => new JobQueueLayerActor(jobStore));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            var set = coordinator.Ask(new SetJobQueueConfiguration(workerProps, queueLayerProps,queueName,1,1,1)).Result;
            coordinator.Tell(new ShutDownQueues());
            ExpectMsgFrom<QueueShutDown>(coordinator);

        }

        [Fact]
        public void JobCoordinator_Will_Not_Fire_OnJobQueueSaturation_For_AggressiveSweep()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            //TODO: Make this less like an integration test; there's no reason we couldn't mock this out with just testprobe.
            var workerCount = 1;
            var jobAdder = AkkaExecutionTest.GetJobQueueAdder;
            var executor = new DefaultJobExecutor(new DefaultContainerFactory());
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new JobQueueLayerActor(AkkaExecutionTest.GetJobQueueManager));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new CountingOnJobQueueSaturatedCoordinator()));
            jobAdder.AddJob((DelayJob j) => j.DoDelay("ar-1"), queueName: queueName);
            jobAdder.AddJob((DelayJob j) => j.DoDelay("ar-2"), queueName: queueName);
            jobAdder.AddJob((DelayJob j) => j.DoDelay("ar-3"), queueName: queueName);
            coordinator.Tell(new SetJobQueueConfiguration(workerProps, queueLayerProps, queueName, 1, 1, 1, aggressiveSweep:true));
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.Equal(2, CountingOnJobQueueSaturatedCoordinator.pulseCount[queueName]);
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("ar-1"));
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("ar-2"));
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("ar-3"));
        }

        [Fact]
        public void JobCoordinator_Will_Fire_OnJobQueueSaturation()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            //TODO: Make this less like an integration test; there's no reason we couldn't mock this out with just testprobe.
            var workerCount = 1;
            var jobAdder = AkkaExecutionTest.GetJobQueueAdder;
            var executor = new DefaultJobExecutor(new DefaultContainerFactory());
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new JobQueueLayerActor(AkkaExecutionTest.GetJobQueueManager));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new CountingOnJobQueueSaturatedCoordinator()));
            jobAdder.AddJob((DelayJob j) => j.DoDelay("qs-1"), queueName:queueName);
            jobAdder.AddJob((DelayJob j) => j.DoDelay("qs-2"),queueName:queueName);
            jobAdder.AddJob((DelayJob j) => j.DoDelay("qs-3"),queueName:queueName);
            coordinator.Tell(new SetJobQueueConfiguration(workerProps, queueLayerProps, queueName, 1, 1, 1));
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(2));
            Xunit.Assert.Equal(2,CountingOnJobQueueSaturatedCoordinator.pulseCount[queueName]);
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("qs-1"));
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("qs-2"));
            Xunit.Assert.False(DelayJob.MsgCounter.ContainsKey("qs-3"));
        }

        [Fact]
        public void JobCoordinator_Will_Sweep_When_Asked()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var workerCount = 5;
            
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe,JobStates.New));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, queueName,1,1,1));
            coordinator.Tell(new JobSweep());
            probe.ExpectMsg<GetJobs>();
        }
        [Fact]
        public void JobCoordinator_Will_Send_Job_To_Queue()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var workerCount = 5;
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe, "Success"));
            var workerProbe = CreateTestProbe("worker");
            var workerProps = Props.Create(() => new MockJobWorker(workerProbe)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, queueName,1,0,5));
            coordinator.Tell(new JobSweep());
            workerProbe.ExpectMsg<ExecuteJobRequest>(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void JobCoordinator_Will_Handle_Failure()
        {
            var workerCount = 5;
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe, "WhoCares"));
            var workerProbe = CreateTestProbe("worker");
            var workerProps = Props.Create(() => new MockJobWorker(workerProbe)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps, queueLayerProps, "WhoCares", 5,1,1));
            coordinator.Tell(new JobFailed(new OddJobWithMetaData(), new Exception("derp")));
            probe.ExpectMsg<MarkJobFailed>(TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void JobCoordinator_Will_Handle_Retry()
        {
            var workerCount = 5;
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe, "WhoCares"));
            var workerProbe = CreateTestProbe("worker");
            var workerProps = Props.Create(() => new MockJobWorker(workerProbe)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps,"WhoCares",5,1,1));
            coordinator.Tell(new JobFailed(new OddJobWithMetaData() { RetryParameters = new RetryParameters(5, TimeSpan.FromSeconds(10)) }, new Exception("derp")));
            probe.ExpectMsg<MarkJobInRetryAndIncrement>(TimeSpan.FromSeconds(5));
        }
        [Fact]
        public void JobCoordinator_Will_Handle_Final_Retry_failure()
        {
            var workerCount = 5;
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe, "WhoCares"));
            var workerProbe = CreateTestProbe("worker");
            var workerProps = Props.Create(() => new MockJobWorker(workerProbe)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, "WhoCares",5,1,1));
            coordinator.Tell(new JobFailed(new OddJobWithMetaData() { RetryParameters = new RetryParameters(5, TimeSpan.FromSeconds(10)) { RetryCount = 5 } }, new Exception("derp")));
            probe.ExpectMsg<MarkJobFailed>(TimeSpan.FromSeconds(5));
        }
    }
}
