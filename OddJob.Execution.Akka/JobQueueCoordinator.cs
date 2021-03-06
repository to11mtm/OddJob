﻿using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Routing;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
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
        public int PendingItems { get; protected set; }
        public DateTime? SaturationStartTime { get; protected set; }
        public int SaturationPulseCount { get; protected set; }
        public long QueueLifeSaturationPulseCount { get; protected set; }
        public Props WorkerProps { get; protected set; }
        public bool AggressiveSweep { get; protected set; }
        public JobQueueCoordinator()
        {
            ShuttingDown = false;
            PendingItems = 0;
            QueueLifeSaturationPulseCount = 0;
        }
        protected override bool Receive(object message)
        {
            if (message is SetJobQueueConfiguration)
            {
                var config = message as SetJobQueueConfiguration;
                WorkerCount = config.NumWorkers;
                QueueName = config.QueueName;
                JobQueueActor = Context.ActorOf(config.QueueProps, "jobQueue");
                WorkerProps = config.WorkerProps;
                AggressiveSweep = config.AggressiveSweep;
                WorkerRouterRef = Context.ActorOf(WorkerProps.WithRouter(new RoundRobinPool(WorkerCount)), "workerPool");
                Context.Sender.Tell(new Configured());
            }
            else if (message is ShutDownQueues)
            {
                ShutdownRequester = Context.Sender;
                ShuttingDown = true;
                //Tell all of the children to stop what they're doing.
                WorkerRouterRef.Tell(new Broadcast(new ShutDownQueues()));
            }
            else if (message is QueueShutDown)
            {
                ShutdownCount = ShutdownCount + 1;
                SaturationPulseCount = 0;
                if (ShutdownCount == WorkerCount)
                {
                    //We Do this ask to make sure that all DB commands from the queues have been flushed.
                    var storeShutdown = JobQueueActor.Ask(new ShutDownQueues()).Result as QueueShutDown;
                    //Tell our requester that we are truly done.
                    ShutdownRequester.Tell(new QueueShutDown());
                }
            }
            else if ((message is JobSweep || message is SilentRetrySweep) && !ShuttingDown)
            {
                HandleSweep(message);
            }
            else if (message is JobSuceeded)
            {
                var msg = (JobSuceeded)message;
                JobQueueActor.Tell(new MarkJobSuccess(msg.JobData.JobId));
                PendingItems = PendingItems - 1;
                try
                {
                    OnJobSuccess(msg);
                }
                catch(Exception ex)
                {
                    Context.System.Log.Error(ex, "Error Running OnJobSuccess Handler for Queue {0}, job {1}", QueueName, msg.JobData.JobId);
                }
            }
            else if (message is JobFailed)
            {
                var msg = message as JobFailed;
                PendingItems = PendingItems - 1;
                if (msg.JobData.RetryParameters == null || msg.JobData.RetryParameters.MaxRetries <= msg.JobData.RetryParameters.RetryCount)
                {
                    JobQueueActor.Tell(new MarkJobFailed(msg.JobData.JobId));

                    try
                    {
                        OnJobFailed(msg);
                    }
                    catch (Exception ex)
                    {
                        Context.System.Log.Error(ex, "Error Running OnJobFailed Handler for Queue {0}, job {1}", QueueName, msg.JobData.JobId);
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
                        Context.System.Log.Error(ex, "Error Running OnJobRetry Handler for Queue {0}, job {1}", QueueName, msg.JobData.JobId);
                    }
                }
            }
            else
            {
                return OnCustomMessage(message);
            }
            return true;
        }

        private void HandleSweep(object message)
        {
            //Naieve Backpressure:
            if (PendingItems < WorkerCount * 2)
            {
                SaturationStartTime = null;
                SaturationPulseCount = 0;
                IEnumerable<IOddJobWithMetadata> jobsToQueue = null;
                try
                {
                    jobsToQueue =
                        JobQueueActor.Ask(new GetJobs(QueueName, WorkerCount), TimeSpan.FromSeconds(30)).Result as
                            IEnumerable<IOddJobWithMetadata>;
                }
                catch (Exception ex)
                {
                    Context.System.Log.Error(ex, "Timeout Retrieving data for Queue {0}", QueueName);
                    try
                    {
                        OnQueueTimeout(ex);
                    }
                    catch (Exception)
                    {
                        Context.System.Log.Error(ex, "Error Running OnQueueTimeout Handler for Queue {0}",
                            QueueName);
                    }
                }

                if (jobsToQueue != null)
                {
                    foreach (var job in jobsToQueue)
                    {
                        if (job.TypeExecutedOn == null)
                        {
                            try
                            {
                                JobQueueActor.Tell(new MarkJobFailed(job.JobId));
                                OnJobTypeMissing(job);
                            }
                            catch (Exception ex)
                            {
                                Context.System.Log.Error(ex, "Error Running OnQueueTimeout Handler for Queue {0}", QueueName);
                            }
                        }
                        else
                        {
                            WorkerRouterRef.Tell(new ExecuteJobRequest(job));
                            JobQueueActor.Tell(new MarkJobInProgress(job.JobId));
                            PendingItems = PendingItems + 1;
                        }
                    }
                }
            }
            else
            {
                if (message is JobSweep)
                {
                    SaturationStartTime = SaturationStartTime ?? DateTime.Now;
                    SaturationPulseCount = SaturationPulseCount + 1;
                    QueueLifeSaturationPulseCount = QueueLifeSaturationPulseCount + 1;
                    try
                    {
                        OnJobQueueSaturated(SaturationStartTime.Value, SaturationPulseCount,
                            QueueLifeSaturationPulseCount);
                    }
                    catch (Exception ex)
                    {
                        Context.System.Log.Error(ex,
                            "Error Running OnJobQueueSaturated Handler for Queue {0}, Saturation Start Time : {1}, number of Saturated pulses {2}, Total Saturated Pulses for Life of Queue: {3}",
                            QueueName, SaturationStartTime.ToString(), SaturationPulseCount,
                            QueueLifeSaturationPulseCount);
                    }
                }

                if (AggressiveSweep)
                {
                    Context.Self.Tell(new SilentRetrySweep());
                }
            }
        }

        /// <summary>
        /// An Extension point to allow special handling of logic here.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool OnCustomMessage(object message)
        {
            return false;
        }

        /// <summary>
        /// Method to handle Missing Types on a Job.
        /// If a Job's type is missing, it will be marked in an error state (to prevent queue backup)
        /// But this method will let you specify a specific sort of warning/handler for the issue.
        /// </summary>
        /// <param name="job">The job missing a type.</param>
        protected virtual void OnJobTypeMissing(IOddJobWithMetadata job)
        {
            
        }

        /// <summary>
        /// Method to handle Queue Read Failures(e.x. Timeouts).
        /// This can be used to do things like send email, perhaps trigger a Queue shutdown, etc.
        /// </summary>
        /// <param name="ex">The Queue Failure recieved.</param>
        protected virtual void OnQueueTimeout(Exception ex)
        {

        }
        /// <summary>
        /// Method to handle action taken when a job has suceeded.
        /// This method is called after the success has been marked in storage.
        /// </summary>
        /// <param name="msg">the Job success</param>
        protected virtual void OnJobSuccess(JobSuceeded msg)
        {

        }


        /// <summary>
        /// Method to handle action taken when a job is put in a Failed state.
        /// This method is called after the retry has been marked in storage.
        /// </summary>
        /// <param name="msg">the Job failure message</param>
        protected virtual void OnJobFailed(JobFailed msg)
        {

        }
        /// <summary>
        /// Method to handle action taken when a job is put in retry.
        /// This method is called after the retry has been marked in storage.
        /// </summary>
        /// <param name="msg">the Job retry message</param>
        protected virtual void OnJobRetry(JobFailed msg)
        {

        }

        /// <summary>
        /// Method to handle Job Queue Saturation;
        /// As an example, if you want an Email or other notification sent when the queue is saturated. 
        /// The time saturation started as well as the number of missed pulses are provided for use of threshholds.
        /// e.x. Send an email when you have had a saturated queue for more than 10 minutes, or have missed more than 10 pulses.
        /// </summary>
        /// <param name="saturationTime">The time Saturation initially started</param>
        /// <param name="saturationMissedPulseCount">The number of pulses that have been missed due to saturation.</param>
        /// <param name="queueLifeSaturationPulseCount">The total number of pulses that have been missed over the life of the queue.</param>
        protected virtual void OnJobQueueSaturated(DateTime saturationTime, int saturationMissedPulseCount, long queueLifeSaturationPulseCount)
        {

        }
    }
}

