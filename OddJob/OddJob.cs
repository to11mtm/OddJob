using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public class OddJob : IOddJob
    {
        /// <summary>
        /// The Job ID. Uniquely generated when the job is created.
        /// </summary>
        public Guid JobId { get; protected set; }

        /// <summary>
        /// Array of Job arguments.
        /// </summary>
        public OddJobParameter[] JobArgs { get; protected set; }

        /// <summary>
        /// Gets the Type the Job is Executed on.
        /// This will include any Generic parameters on the type (not method) in question
        /// </summary>
        public Type TypeExecutedOn { get; protected set; }

        /// <summary>
        /// Gets the Method name for the Job
        /// </summary>
        public string MethodName { get; protected set; }

        /// <summary>
        /// Indicates the current Status for the Job.
        /// </summary>
        public string Status { get; protected set; }

        /// <summary>
        /// Contains the Generic Types used by the method.
        /// </summary>
        public Type[] MethodGenericTypes { get; protected set; }

        internal OddJob(Guid jobId, string methodName,
            OddJobParameter[] jobArgs,
            Type typeExecutedOn, Type[] methodGenericTypes,
            string status = JobStates.New)
        {
            JobId = jobId;
            MethodName = methodName;
            JobArgs = jobArgs;
            TypeExecutedOn = typeExecutedOn;
            Status = status;
            MethodGenericTypes = methodGenericTypes;
        }

        public OddJob(string methodName, OddJobParameter[] jobArgs,
            Type typeExecutedOn, Type[] methodGenericTypes,
            string status = JobStates.New) : this(Guid.NewGuid(), methodName,
            jobArgs, typeExecutedOn, methodGenericTypes, status)
        {
        }
    }




}