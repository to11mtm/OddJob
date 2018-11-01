using System;
using System.Linq;
using System.Threading;
using GlutenFree.OddJob.Interfaces;
using Xunit;

namespace GlutenFree.OddJob.Storage.BaseTests
{
    public abstract class StorageTests
    {
        protected abstract Func<IJobQueueAdder> JobAddStoreFunc { get; }
        protected abstract Func<IJobQueueManager> JobMgrStoreFunc { get; }

        [Fact]
        public void Job_can_be_Stored()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            var job = jobMgrStore.GetJob(jobGuid);
            Assert.Equal(TestConstants.SimpleParam, job.JobArgs[0].Value);
            var paramObj = job.JobArgs[1].Value as OddParam;
            Assert.True(paramObj != null);
            Assert.Equal(TestConstants.OddParam1, paramObj.Param1);
            Assert.Equal(TestConstants.OddParam2, paramObj.Param2);
            Assert.True(paramObj.Nested != null);
            Assert.Equal(TestConstants.NestedOddParam1,paramObj.Nested.NestedParam1);
            Assert.Equal(TestConstants.NestedOddParam2, paramObj.Nested.NestedParam2);
        }

        [Fact]
        public void Job_With_Arity_can_be_Stored()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJobWithArity j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            var job = jobMgrStore.GetJob(jobGuid);
            Assert.Equal(TestConstants.SimpleParam, job.JobArgs[0].Value);
            var paramObj = job.JobArgs[1].Value as OddParam;
            Assert.True(paramObj != null);
            Assert.Equal(TestConstants.OddParam1, paramObj.Param1);
            Assert.Equal(TestConstants.OddParam2, paramObj.Param2);
            Assert.True(paramObj.Nested != null);
            Assert.Equal(TestConstants.NestedOddParam1, paramObj.Nested.NestedParam1);
            Assert.Equal(TestConstants.NestedOddParam2, paramObj.Nested.NestedParam2);
            Assert.Equal(typeof(string), job.MethodGenericTypes[0]);
            Assert.Equal(typeof(OddParam),job.MethodGenericTypes[1]);
        }

        [Fact]
        public void Job_In_class_with_Arity_can_be_Stored()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJobInClassWithArity<string> j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            var job = jobMgrStore.GetJob(jobGuid);
            Assert.Equal(TestConstants.SimpleParam, job.JobArgs[0].Value);
            var paramObj = job.JobArgs[1].Value as OddParam;
            Assert.True(paramObj != null);
            Assert.Equal(TestConstants.OddParam1, paramObj.Param1);
            Assert.Equal(TestConstants.OddParam2, paramObj.Param2);
            Assert.True(paramObj.Nested != null);
            Assert.Equal(TestConstants.NestedOddParam1, paramObj.Nested.NestedParam1);
            Assert.Equal(TestConstants.NestedOddParam2, paramObj.Nested.NestedParam2);
            Assert.Equal(typeof(MockJobInClassWithArity<string>),job.TypeExecutedOn);
        }

        [Fact]
        public void Job_With_No_Parameters_can_be_Stored()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJobNoParam j) => j.DoThing());

            var job = jobMgrStore.GetJob(jobGuid);
            Assert.NotNull(job);
            Assert.Equal(typeof(MockJobNoParam), job.TypeExecutedOn);

        }

        [Fact]
        public void Job_With_No_Params_Can_be_Retrieved_from_Store()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var unused = jobAddStore.AddJob(
                (MockJobNoParam j) => j.DoThing(), queueName:"test");

            var jobs = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            
            Assert.Contains(jobs, q =>q.JobId == unused);
            var job = jobs.Where(q => q.JobId == unused).FirstOrDefault();
            Assert.NotNull(job);
            Assert.Equal(typeof(MockJobNoParam), job.TypeExecutedOn);
        }

        [Fact]
        public void Job_Can_be_Retrieved_from_Store()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var unused = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            var job = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            Assert.True(job.Any());
        }

        [Fact]
        public void Jobs_Are_Locked_On_Fetch()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var unused = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");

            var job = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(3));
            var job2 = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            Assert.True(job.Count() != job2.Count());
        }

        [Fact]
        public void Job_Can_Be_Marked_as_success()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobSuccess(jobGuid);
            var job = jobMgrStore.GetJob((jobGuid));
            Assert.Equal(JobStates.Processed, job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Failed()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobFailed(jobGuid);
            var job = jobMgrStore.GetJob((jobGuid));
            Assert.Equal(JobStates.Failed, job.Status);
        }


        [Fact]
        public void Job_Can_Be_Marked_as_InProgess()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobFailed(jobGuid);
            var job = jobMgrStore.GetJob((jobGuid));
            Assert.Equal(JobStates.Failed, job.Status);
        }

        [Fact]
        public void Job_Can_Be_Marked_as_Retry()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), null, null, "test");


            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var job = jobMgrStore.GetJob((jobGuid));
            Assert.Equal(JobStates.Retry, job.Status);
        }

        [Fact]
        public void Job_will_Requeue_Within_Retry_Parameters()
        {
            var jobAddStore = JobAddStoreFunc();
            var jobMgrStore = JobMgrStoreFunc();
            var jobGuid = jobAddStore.AddJob(
                (MockJob j) => j.DoThing(TestConstants.SimpleParam,
                    new OddParam
                    {
                        Param1 = TestConstants.OddParam1,
                        Param2 = TestConstants.OddParam2,
                        Nested = new NestedOddParam
                        {
                            NestedParam1 = TestConstants.NestedOddParam1,
                            NestedParam2 = TestConstants.NestedOddParam2
                        }
                    }), new RetryParameters(1, TimeSpan.Zero), null, "test");


            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfterFirstIncrement = jobMgrStore.GetJob(jobGuid);
            Assert.Equal(1, jobAfterFirstIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterFirstRetry = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            Assert.Contains(jobsAfterFirstRetry, q => q.JobId == jobGuid);
            jobMgrStore.MarkJobInRetryAndIncrement(jobGuid, DateTime.Now);
            var jobAfterSecondIncrement = jobMgrStore.GetJob(jobGuid);
            Assert.Equal(2, jobAfterSecondIncrement.RetryParameters.RetryCount);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
            var jobsAfterSecondRetry = jobMgrStore.GetJobs(new[] { "test" }, 5, q=>q.MostRecentDate);
            Assert.True(jobsAfterSecondRetry.All(q => q.JobId != jobGuid));
        }
    }
}