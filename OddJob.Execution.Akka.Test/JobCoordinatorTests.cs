using System;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using Akka.Actor;
using Akka.Routing;
using Akka.TestKit.Xunit2;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;
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
    public class JobCoordinatorTests : AkkaExecutionTest
    {
        [Fact]
        public void JobCoordinator_Can_Shutdown()
        {
            var workerCount = 5;
            var jobStore = GetJobQueueManager;
            var executor = new MockJobSuccessExecutor();
            var queueLayerProps = Props.Create(() => new JobQueueLayerActor(jobStore));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            var set = coordinator.Ask(new SetJobQueueConfiguration(workerProps, queueLayerProps,"test",1,1,1)).Result;
            coordinator.Tell(new ShutDownQueues());
            ExpectMsgFrom<QueueShutDown>(coordinator);

        }

        [Fact]
        public void JobCoordinator_Will_Sweep_When_Asked()
        {
            var workerCount = 5;
            
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe,JobStates.New));
            var workerProps = Props.Create(() => new JobWorkerActor(executor)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, "test",1,1,1));
            coordinator.Tell(new JobSweep());
            probe.ExpectMsg<GetJobs>();
        }
        [Fact]
        public void JobCoordinator_Will_Send_Job_To_Queue()
        {
            var workerCount = 5;
            var executor = new MockJobSuccessExecutor();
            var probe = CreateTestProbe("queue");
            var queueLayerProps = Props.Create(() => new QueueLayerMock(probe, "Success"));
            var workerProbe = CreateTestProbe("worker");
            var workerProps = Props.Create(() => new MockJobWorker(workerProbe)).WithRouter(new RoundRobinPool(workerCount));
            var coordinator = Sys.ActorOf(Props.Create(() => new JobQueueCoordinator()));
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, "queue",1,0,5));
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
