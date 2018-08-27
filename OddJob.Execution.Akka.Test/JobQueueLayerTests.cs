using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.BaseTests;
using GlutenFree.OddJob.Interfaces;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class JobQueueLayerTests : TestKit
    {
        [Fact]
        public void JobQueueManager_Can_Mark_Success()
        {
            var testStore = new InMemoryTestStore();
            var actor = Sys.ActorOf(Props.Create(() => new JobQueueLayerActor(new InMemoryTestStore())));
            var jobInfo = JobCreator.Create((MockJob m) => m.MockMethod());
            var myGuid = testStore.AddJob((MockJob m) => m.MockMethod(), null, null, "test");
            var ourJob = InMemoryTestStore.jobPeeker["test"].FirstOrDefault(q => q.JobId == myGuid);
            actor.Tell(new MarkJobSuccess(myGuid));
            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(2));
            Xunit.Assert.Equal("Success", ourJob.Status);
        }

        [Fact]
        public void JobQueueManager_Can_Mark_Failure()
        {
            var testStore = new InMemoryTestStore();
            var actor = Sys.ActorOf(Props.Create(() => new JobQueueLayerActor(new InMemoryTestStore())));
            var jobInfo = JobCreator.Create((MockJob m) => m.MockMethod());
            var myGuid = testStore.AddJob((MockJob m) => m.MockMethod(), null, null, "test");
            var ourJob = InMemoryTestStore.jobPeeker["test"].FirstOrDefault(q => q.JobId == myGuid);
            actor.Tell(new MarkJobFailed(myGuid));
            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(2));
            Xunit.Assert.Equal(JobStates.Failed, ourJob.Status);
        }

        [Fact]
        public void JobQueueManager_Can_Mark_Retry()
        {
            var testStore = new InMemoryTestStore();
            var actor = Sys.ActorOf(Props.Create(() => new JobQueueLayerActor(new InMemoryTestStore())));
            var jobInfo = JobCreator.Create((MockJob m) => m.MockMethod());
            var myGuid = testStore.AddJob((MockJob m) => m.MockMethod(), new RetryParameters(1, TimeSpan.FromSeconds(20)), null, "test");
            var ourJob = InMemoryTestStore.jobPeeker["test"].FirstOrDefault(q => q.JobId == myGuid);
            var attemptTime = DateTime.Now;
            actor.Tell(new MarkJobInRetryAndIncrement(myGuid, attemptTime));
            System.Threading.SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(2));
            Xunit.Assert.Equal(JobStates.Retry, ourJob.Status);
            Xunit.Assert.Equal(1, ourJob.RetryParameters.RetryCount);
            Xunit.Assert.Equal(attemptTime, ourJob.RetryParameters.LastAttempt);
        }

        [Fact]
        public void JobQueueManager_Can_Get_Jobs()
        {
            var testStore = new InMemoryTestStore();
            var actor = Sys.ActorOf(Props.Create(() => new JobQueueLayerActor(new InMemoryTestStore())));
            
            var myGuid = testStore.AddJob((MockJob m) => m.MockMethod(), null, null, "test");
            InMemoryTestStore.jobPeeker["test"].FirstOrDefault(q => q.JobId == myGuid);
            

            actor.Tell(new GetJobs("test", 10));

            ExpectMsg<IEnumerable<IOddJobWithMetadata>>(duration: TimeSpan.FromSeconds(180));
        }
    }
}
