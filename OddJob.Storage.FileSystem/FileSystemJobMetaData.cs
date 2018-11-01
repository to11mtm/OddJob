using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Storage.FileSystem
{
    public class FileSystemJobMetaData : IOddJobWithMetadata
    {
        public RetryParameters RetryParameters { get; set; }

        public Guid JobId {get;set;}

        public OddJobParameter[] JobArgs {get;set;}

        public Type TypeExecutedOn {get;set;}

        public string MethodName {get;set;}
        public string QueueName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Status { get; set; }
        public Type[] MethodGenericTypes { get; set; }
        public DateTime QueueTime { get; internal set; }
        public DateTime FailureTime { get; internal set; }
        public DateTime LastAttemptTime { get; internal set; }

        IRetryParameters IOddJobWithMetadata.RetryParameters
        {
            get
            {
                return RetryParameters;
            }
        }
    }
}
