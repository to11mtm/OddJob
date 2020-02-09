using System;
using System.Threading;
using GlutenFree.OddJob.Execution.Akka.Test.Mocks;
using GlutenFree.OddJob.Execution.BaseTests;
using GlutenFree.OddJob.Interfaces;
using Moq;
using Xunit;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class WriterPluginTests
    {
        [Fact]
        public void Plugin_Writes()
        {
            var queueName = QueueNameHelper.CreateQueueName();
            var jobStore = new InMemoryTestStore();
            var executor =
                new HardInjectedJobExecutorShell<JobQueueLayerActor,
                    JobWorkerActor, JobQueueCoordinator>(
                    () => new JobQueueLayerActor(jobStore),
                    () => new JobWorkerActor(new MockJobSuccessExecutor()),
                    () => new JobQueueCoordinator(), null);
            var mockJobQueueWriter = new Mock<IJobQueueResultWriter>();
            var written = false;

            jobStore.AddJob((StaticClassJob j) => string.Concat("lol"),
                queueName:queueName);
            var mockObj = mockJobQueueWriter.Object;
            executor.StartJobQueue(queueName, 5, 1, firstPulseDelayInSeconds:1,
                plugins: new IJobExecutionPluginConfiguration[]
                {
                    ResultRecordingPlugin.PropFactory(() => mockObj, 1),
                });

            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));

            mockJobQueueWriter.Verify(
                r => r.WriteJobQueueResult(It.IsAny<Guid>(),
                    It.IsAny<IOddJobResult>()), Times.Once);

        }
    }
}