using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dapper;
using Xunit;
using Xunit.Abstractions;
using Timer = System.Timers.Timer;

namespace OddJob.Storage.SqlServer.Test
{
    public class UnitTest1
    {
        public UnitTest1()
        {
            var execPath = Path.Combine(Assembly.GetExecutingAssembly().CodeBase, string.Empty)
                .Substring(0, Assembly.GetExecutingAssembly().CodeBase.LastIndexOf('/'));
            AppDomain.CurrentDomain.SetData("DataDirectory", new Uri(Path.Combine(execPath, string.Empty)).LocalPath);
            UnitTestTableHelper.EnsureTablesExist();
        }

        [Fact]
        public void Job_can_be_Stored()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");
            var conn = SqlConnectionHelper.GetLocalDB("unittestdb");

            var job = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(TestConstants.SimpleParam, job.JobArgs[0]);
        }

        [Fact]
        public void Job_Can_be_Retrieved_from_Store()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");
            var conn = SqlConnectionHelper.GetLocalDB("unittestdb");
            var results = conn.Query(string.Format("select * from {0}",
                SqlServerJobQueueDefaultTableConfiguration.DefaultQueueTableName));

            var job = jobMgrStore.GetJobs(new[] {"test"}, 5);
            Xunit.Assert.True(job.Count()>0);
        }

        [Fact]
        public void Jobs_Are_Locked_On_Fetch()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");
            var conn = SqlConnectionHelper.GetLocalDB("unittestdb");

            var job = jobMgrStore.GetJobs(new[] { "test" }, 5);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(3));
            var job2 = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(job.Count() != job2.Count());
        }

        [Fact]
        public void Job_Can_Be_Marked_as_success()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            
            jobMgrStore.MarkJobSuccess(jobGuid);
            var job = jobMgrStore.GetJob((jobGuid));
            Xunit.Assert.Equal("Processed",job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Failed()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobFailed(jobGuid);
            var job = jobMgrStore.GetJob((jobGuid));
            Xunit.Assert.Equal("Failed", job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Retry()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var job = jobMgrStore.GetJob((jobGuid));
            Xunit.Assert.Equal("Retry", job.Status);
        }

        [Fact]
        public void Job_will_Requeue_Within_Retry_Parameters()
        {
            var jobAddStore = new SqlServerJobQueueAdder(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobMgrStore = new SqlServerJobQueueManager(new TestConnectionFactory(),
                new SqlServerJobQueueDefaultTableConfiguration());
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam()
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam()
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }),new RetryParameters(1, TimeSpan.Zero), null, "test");

            
            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfter1stIncrement = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(1,jobAfter1stIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterFirstRetry = jobMgrStore.GetJobs(new[] {"test"}, 5);
            Xunit.Assert.True(jobsAfterFirstRetry.Any(q=>q.JobId == jobGuid));
            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfter2ndstIncrement = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(2, jobAfter2ndstIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterSecondRetry = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(jobsAfterSecondRetry.All(q => q.JobId != jobGuid));
        }
    }

    public class TestConstants
    {
        public const string SimpleParam = "simple";
        public const string OddParam1 = "odd1";
        public const string OddParam2 = "odd2";
        public const string NestedOddParam1 = "n1";
        public const string NestedOddParam2 = "n2";
    }

    public class MockJob
    {
        public void DoThing(string simpleParam, OddParam oddParam)
        {

        }
    }

    public class OddParam
    {
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public NestedOddParam Nested { get; set; }
    }

    public class NestedOddParam
    {
        public string NestedParam1 { get; set; }
        public string NestedParam2 { get; set; }
    }
}


