using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
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
        IRetryParameters IOddJobWithMetadata.RetryParameters { get { return RetryParameters; } }
        public DateTimeOffset? ExecutionTime { get; set; }
        public string Queue { get; set; }
    }
}