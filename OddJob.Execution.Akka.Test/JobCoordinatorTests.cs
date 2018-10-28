using System;
using Akka.Actor;
using Akka.Routing;
using Akka.TestKit.Xunit2;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class JobCoordinatorTests : TestKit
    {
        [Fact]
        public void JobCoordinator_Can_Shutdown()
        {
            var workerCount = 5;
            var jobStore = new InMemoryTestStore();
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
