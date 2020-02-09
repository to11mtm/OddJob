using System;
using System.Threading;
using Akka.Actor;
using Akka.Routing;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
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
            coordinator.Tell(new SetJobQueueConfiguration(workerProps, queueLayerProps, queueName, 1, 1, 1, aggressiveSweep:true, allowedPendingSweeps:0));
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Xunit.Assert.True(0< CountingOnJobQueueSaturatedCoordinator.pulseCount[queueName]);
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
            coordinator.Tell(new SetJobQueueConfiguration(workerProps, queueLayerProps, queueName, 1, 1, 1, allowedPendingSweeps:0));
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            coordinator.Tell(new JobSweep());
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(3));
            Xunit.Assert.True(0<CountingOnJobQueueSaturatedCoordinator.pulseCount[queueName]);
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("qs-1"));
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("qs-2"));
            Xunit.Assert.True(DelayJob.MsgCounter.ContainsKey("qs-3"));
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
            coordinator.Tell(new SetJobQueueConfiguration(workerProps,queueLayerProps, queueName,1,5));
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
