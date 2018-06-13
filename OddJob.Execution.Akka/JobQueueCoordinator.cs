using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OddJob.Execution.Akka
{
    public class JobQueueCoordinator : ActorBase
    {
        public IActorRef WorkerRouterRef { get; protected set; }
        public IActorRef JobQueueActor { get; protected set; }
        public string QueueName { get; protected set; }
        public bool ShuttingDown { get; protected set; }
        public int WorkerCount { get; protected set; }
        public int ShutdownCount { get; protected set; }
        public IActorRef ShutdownRequester { get; protected set; }
        public JobQueueCoordinator(Props workerProps, Props jobQueueActorProps, string queueName,int workerCount)
        {
            QueueName = queueName;
            JobQueueActor = Context.ActorOf(jobQueueActorProps,"jobQueue");
            WorkerRouterRef = Context.ActorOf(workerProps,"workerPool");
            ShuttingDown = false;
            WorkerCount = workerCount;
        }
        protected override bool Receive(object message)
        {
            if (message is ShutDownQueues)
            {
                ShutdownRequester = Context.Sender;
                ShuttingDown = true;
                JobQueueActor.Tell(new Broadcast(new ShutDownQueues()));
            }
            if (message is QueueShutDown)
            {
                ShutdownCount = ShutdownCount + 1;
                if (ShutdownCount == WorkerCount)
                {
                    ShutdownRequester.Tell(new QueueShutDown());
                }
            }
            if (message is JobSweep && !ShuttingDown)
            {
                var jobsToQueue = JobQueueActor.Ask(new GetJobs(QueueName)).Result as IEnumerable<IOddJobWithMetadata>;
                foreach (var job in jobsToQueue)
                {
                    WorkerRouterRef.Tell(new ExecuteJobRequest(job));
                    JobQueueActor.Tell(new MarkJobInProgress(job.JobId));
                }
            }
            else if (message is JobSuceeded)
            {
                var msg = (JobSuceeded)message;
                JobQueueActor.Tell(new MarkJobSuccess(msg.JobData.JobId));
                OnJobSuccess(msg);
            }
            else if (message is JobFailed)
            {
                var msg = message as JobFailed;
                if (msg.JobData.RetryParameters.MaxRetries >= msg.JobData.RetryParameters.RetryCount)
                {
                    JobQueueActor.Tell(new MarkJobFailed(msg.JobData.JobId));

                    try
                    {
                        OnJobFailed(msg);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    JobQueueActor.Tell(new MarkJobInRetryAndIncrement(msg.JobData.JobId, DateTime.Now));
                    try
                    {
                        OnJobRetry(msg);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        protected virtual void OnJobSuccess(JobSuceeded msg)
        {

        }

        protected virtual void OnJobFailed(JobFailed msg)
        {

        }

        protected virtual void OnJobRetry(JobFailed msg)
        {

        }
    }
}

