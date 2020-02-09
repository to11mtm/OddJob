using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using Akka.Util.Internal;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    /// <summary>
    /// A Job Queue Coordinator For handling jobs and their logic.
    /// </summary>
    public class JobQueueCoordinator : ActorBase
    {
        protected List<IActorRef> PluginRefs { get; set; }
        protected int PluginCount { get;  set; }
        protected int PluginShutdownCount { get; set; }
        public IActorRef WorkerRouterRef { get; protected set; }
        public IActorRef JobQueueReaderRef { get; protected set; }
        public IActorRef JobQueueWritersRef { get; protected set; }
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
        public int AllowedPendingSweeps { get; protected set; }
        public Dictionary<Guid, DateTime> PendingSweeps { get; } = new Dictionary<Guid, DateTime>();
        public int PendingSweepTimeoutSeconds { get; protected set; }
        public JobQueueCoordinator()
        {
            PluginRefs = new List<IActorRef>();
            PluginCount = 0;
            ShuttingDown = false;
            PendingItems = 0;
            QueueLifeSaturationPulseCount = 0;
        }
        protected void RegisterPlugin(IActorRef pluginRef)
        {
            PluginRefs.Add(pluginRef);
            PluginCount = PluginCount +1;
        }
        protected override bool Receive(object message)
        {
            try
            {

            
            if (message is SetJobQueueConfiguration)
            {
                SetConfiguration(message as SetJobQueueConfiguration);
            }
            else if (message is ShutDownQueues)
            {
                StartQueueShutdown();
            }
            else if (message is QueueShutDown)
            {
                HandleQueueShutdownMessage();
            }
            else if (message is GetSpecificJob && !ShuttingDown)
            {
                HandleSpecificJob(message);
            }
            else if ((message is JobSweep || message is SilentRetrySweep) && !ShuttingDown)
            {
                HandleSweep(message);
            }
            else if (message is JobSweepResponse)
            {
                HandleJobSet((JobSweepResponse)message);
            }
            else if (message is ExecuteJobCommand jobCommand)
            {
                HandleExecuteJobItem(jobCommand.Job);
            }
            else if (message is JobSuceeded)
            {
                HandleJobSuccess((JobSuceeded)message);
            }
            else if (message is JobFailed)
            {
                HandleJobFailed((JobFailed)message);
            }
            else
            {
                return OnCustomMessage(message);
            }
            return true;
            }
            catch (Exception e)
            {
                Context.GetLogger().Error(e, "Error Running Handler!");
                throw;
            }
        }

        private void HandleJobFailed(JobFailed msg)
        {
            PendingItems = PendingItems - 1;
            if (msg.JobData.RetryParameters == null ||
                msg.JobData.RetryParameters.MaxRetries <=
                msg.JobData.RetryParameters.RetryCount)
            {
                JobQueueWritersRef.Tell(new MarkJobFailed(msg.JobData.JobId));

                try
                {
                    PluginRefs.ForEach(r => r.Tell(msg));
                    OnJobFailed(msg);
                }
                catch (Exception ex)
                {
                    Context.System.Log.Error(ex,
                        "Error Running OnJobFailed Handler for Queue {0}, job {1}",
                        QueueName, msg.JobData.JobId);
                }
            }
            else
            {
                JobQueueWritersRef.Tell(
                    new MarkJobInRetryAndIncrement(msg.JobData.JobId, DateTime.Now));
                try
                {
                    OnJobRetry(msg);
                }
                catch (Exception ex)
                {
                    Context.System.Log.Error(ex,
                        "Error Running OnJobRetry Handler for Queue {0}, job {1}",
                        QueueName, msg.JobData.JobId);
                }
            }
        }

        private void HandleJobSet(JobSweepResponse jobset)
        {
            var jobsToQueue = jobset.Jobs;
            foreach (var job in jobsToQueue)
            {
                if (job.TypeExecutedOn == null)
                {
                    try
                    {
                        JobQueueWritersRef.Tell(new MarkJobFailed(job.JobId));
                        OnJobTypeMissing(job);
                    }
                    catch (Exception ex)
                    {
                        Context.System.Log.Error(ex, "Error Running OnJobTypeMissing Handler for Queue {0}", QueueName);
                    }
                }
                else
                {
                    HandleExecuteJobItem(job);
                }
            }
        }

        private void HandleJobSuccess(JobSuceeded msg)
        {
            JobQueueWritersRef.Tell(new MarkJobSuccess(msg.JobData.JobId));
            PendingItems = PendingItems - 1;
            try
            {
                PluginRefs.ForEach(r => r.Tell(msg));
                OnJobSuccess(msg);
            }
            catch (Exception ex)
            {
                Context.System.Log.Error(ex,
                    "Error Running OnJobSuccess Handler for Queue {0}, job {1}",
                    QueueName, msg.JobData.JobId);
            }
        }

        private void StartQueueShutdown()
        {
            ShutdownRequester = Context.Sender;
            ShuttingDown = true;
            //Tell all of the children to stop what they're doing.
            WorkerRouterRef.Tell(new Broadcast(new ShutDownQueues()));
        }

        private void SetConfiguration(SetJobQueueConfiguration config)
        {
            WorkerCount = config.NumWorkers;
            WriterCount = config.NumWriters > 0
                ? config.NumWriters
                : config.NumWorkers;

            QueueName = config.QueueName;
            JobQueueReaderRef = Context.ActorOf(config.QueueProps, "jobQueue");
            JobQueueWritersRef = Context.ActorOf(
                config.QueueProps.WithRouter(
                    new RoundRobinPool(WriterCount)), "writerQueue");
            WorkerProps = config.WorkerProps;
            AggressiveSweep = config.AggressiveSweep;
            AllowedPendingSweeps = config.AllowedPendingSweeps;
            PendingSweepTimeoutSeconds = config.PendingSweepTimeoutSeconds;
            WorkerRouterRef =
                Context.ActorOf(WorkerProps.WithRouter(new RoundRobinPool(WorkerCount)),
                    "workerPool");
            config.ExecutionPlugins.ToList()
                .Select((r, i) => new {conf = r, num = i}).ForEach(r =>
                {
                    var created = Context.ActorOf(r.conf.CreationProps,
                        $"plugin-{r.num}-{r.conf.CreationProps.Type.Name}");
                    RegisterPlugin(created);
                    created.Tell(r.conf.ConfigMsg);
                    created.Tell(config);
                });
            Context.Sender.Tell(new Configured());
        }

        protected void HandleQueueShutdownMessage()
        {
            SaturationPulseCount = 0;
            if (ShutdownCount < WorkerCount)
            {
                ShutdownCount = ShutdownCount + 1;
                if (ShutdownCount == WorkerCount)
                {
                    JobQueueReaderRef.Tell(new ShutDownQueues());
                }
            }
            else if (ShutdownCount < WorkerCount + 1)
            {
                ShutdownCount = ShutdownCount + 1;
                if (ShutdownCount == WorkerCount + 1)
                {
                    JobQueueWritersRef.Tell(new Broadcast(new ShutDownQueues()));
                }
            }
            else if (ShutdownCount < WorkerCount + WriterCount + 1)
            {
                ShutdownCount = ShutdownCount + 1;
                if (ShutdownCount == WorkerCount + WriterCount + 1)
                {
                    PluginRefs.ForEach(r => r.Tell(new ShutDownQueues()));
                }
            }
            else if (ShutdownCount >= WorkerCount + WriterCount + 1)
            {
                PluginShutdownCount = PluginShutdownCount + 1;
            }

            if (ShutdownCount >= WorkerCount + WriterCount + 1)
            {
                if (PluginCount == PluginShutdownCount)
                {
                    //Tell our requester that we are truly done.
                    ShutdownRequester.Tell(new QueueShutDown());
                }
            }
        }

        public int WriterCount { get; set; }

        private void HandleSpecificJob(object message)
        {
            if (PendingItems < WorkerCount * 2)
            {
                JobQueueReaderRef.Tell(message);
            }
        }
        private void HandleSweep(object message)
        {
            //Naieve Backpressure:
            var timedOutSweeps =
                PendingSweeps.Where(t => t.Value < DateTime.Now).ToList();
            foreach (var sweep in timedOutSweeps)
            {
                Context.System.Log.Error( "Timeout Retrieving data for Queue {0}", QueueName);
                try
                {
                    OnQueueTimeout(sweep.Key, sweep.Value);
                }
                catch (Exception ex)
                {
                    Context.System.Log.Error(ex, "Error Running OnQueueTimeout Handler for Queue {0}",
                        QueueName);
                }
                PendingSweeps.Remove(sweep.Key);
            }
            if (PendingItems < WorkerCount * 2 &&(AllowedPendingSweeps>= PendingSweeps.Count ))
            {
                SaturationStartTime = null;
                SaturationPulseCount = 0;
                
                    var sweepGuid = Guid.NewGuid();
                    PendingSweeps.Add(sweepGuid,DateTime.Now.AddSeconds(PendingSweepTimeoutSeconds));
                        JobQueueReaderRef.Tell(
                            new GetJobs(QueueName, WorkerCount, sweepGuid));
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

        private void HandleExecuteJobItem(IOddJobWithMetadata job)
        {
            WorkerRouterRef.Tell(new ExecuteJobRequest(job));
            JobQueueWritersRef.Tell(new MarkJobInProgress(job.JobId));
            PendingItems = PendingItems + 1;
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
        /// <param name="requestGuid"></param>
        /// <param name="ex">The Queue Failure recieved.</param>
        /// <param name="expirationTime"></param>
        protected virtual void OnQueueTimeout(Guid requestGuid,
            DateTime expirationTime)
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

