using System;
using System.Linq;
using System.Threading;
using GlutenFree.OddJob.Interfaces;
using Xunit;

namespace GlutenFree.OddJob.Storage.BaseTests
{
    public abstract class StorageTests
    {
        protected abstract Func<IJobQueueAdder> jobAddStoreFunc { get; }
        protected abstract Func<IJobQueueManager> jobMgrStoreFunc { get; }

        [Fact]
        public void Job_can_be_Stored()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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

            var job = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(TestConstants.SimpleParam, job.JobArgs[0]);
        }

        [Fact]
        public void Job_Can_be_Retrieved_from_Store()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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

            var job = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(job.Count() > 0);
        }

        [Fact]
        public void Jobs_Are_Locked_On_Fetch()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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

            var job = jobMgrStore.GetJobs(new[] { "test" }, 5);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(3));
            var job2 = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(job.Count() != job2.Count());
        }

        [Fact]
        public void Job_Can_Be_Marked_as_success()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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
            Xunit.Assert.Equal(JobStates.Processed, job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Failed()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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
            Xunit.Assert.Equal(JobStates.Failed, job.Status);
        }


        [Fact]
        public void Job_Can_Be_Marked_as_InProgess()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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
            Xunit.Assert.Equal(JobStates.Failed, job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Retry()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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
            Xunit.Assert.Equal(JobStates.Retry, job.Status);
        }

        [Fact]
        public void Job_will_Requeue_Within_Retry_Parameters()
        {
            var jobAddStore = jobAddStoreFunc();
            var jobMgrStore = jobMgrStoreFunc();
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
                    }), new RetryParameters(1, TimeSpan.Zero), null, "test");


            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfter1stIncrement = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(1, jobAfter1stIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterFirstRetry = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(jobsAfterFirstRetry.Any(q => q.JobId == jobGuid));
            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfter2ndstIncrement = jobMgrStore.GetJob(jobGuid);
            Xunit.Assert.Equal(2, jobAfter2ndstIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterSecondRetry = jobMgrStore.GetJobs(new[] { "test" }, 5);
            Xunit.Assert.True(jobsAfterSecondRetry.All(q => q.JobId != jobGuid));
        }
    }
}