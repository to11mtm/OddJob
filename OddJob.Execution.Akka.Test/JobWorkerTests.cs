using Akka.Actor;
using Akka.TestKit.Xunit2;
using System;
using Xunit;

namespace OddJob.Execution.Akka.Test
{
    public class JobWorkerTests : TestKit
    {
        [Fact]
        public void JobWorker_Returns_Success_for_Passed_Jobs()
        {
            var actor = Sys.ActorOf(Props.Create(() => new JobWorkerActor(new MockJobSuccessExecutor())));
            var jobInfo = JobCreator.Create((MockJob m) => m.MockMethod());
            var jobData = new OddJobWithMetaData() { Status = JobStates.New, JobArgs = jobInfo.JobArgs, JobId = Guid.NewGuid(), MethodName = jobInfo.MethodName, CreatedOn = DateTime.Now, QueueTime = DateTime.Now, TypeExecutedOn = jobInfo.TypeExecutedOn };
            actor.Tell(new ExecuteJobRequest(jobData));
            ExpectMsg<JobSuceeded>();
        }

        [Fact]
        public void JobWorker_Returns_Failures_for_Failed_Jobs()
        {
            var actor = Sys.ActorOf(Props.Create(() => new JobWorkerActor(new MockJobFailureExecutor())));
            var jobInfo = JobCreator.Create((MockJob m) => m.MockMethod());
            var jobData = new OddJobWithMetaData() { Status = JobStates.New, JobArgs = jobInfo.JobArgs, JobId = Guid.NewGuid(), MethodName = jobInfo.MethodName, CreatedOn = DateTime.Now, QueueTime = DateTime.Now, TypeExecutedOn = jobInfo.TypeExecutedOn };
            actor.Tell(new ExecuteJobRequest(jobData));
            ExpectMsg<JobFailed>();
        }

    }
}
