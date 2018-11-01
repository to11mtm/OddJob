using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.BaseTests
{
    public class OddJobWithMetaData : IOddJobWithMetadata
    {
        public RetryParameters RetryParameters { get; set; }

        public Guid JobId { get; set; }

        public OddJobParameter[] JobArgs { get; set; }

        public Type TypeExecutedOn { get; set; }

        public string MethodName { get; set; }
        public string Status { get; set; }
        public Type[] MethodGenericTypes { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime QueueTime { get; set; }
        public DateTime? LastAttemptTime { get; set; }
        public DateTime? FailureTime { get; set; }
        IRetryParameters IOddJobWithMetadata.RetryParameters { get { return RetryParameters; } }
    }
}
