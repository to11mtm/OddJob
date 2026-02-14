using System;
using GlutenFree.OddJob.Execution.Akka.Messages;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public partial class JobQueueCoordinator
    {
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
        protected virtual void OnJobQueueSaturated(DateTime saturationTime, int saturationMissedPulseCount,
            long queueLifeSaturationPulseCount)
        {
        }
    }
}