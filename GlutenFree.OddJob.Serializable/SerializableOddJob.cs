using System;
using System.Linq;

namespace GlutenFree.OddJob.Serializable
{
    public class SerializableOddJob
    {
        public SerializableOddJob()
        {

        }
        public SerializableOddJob(OddJob job, RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            JobId = job.JobId;
            MethodName = job.MethodName;
            JobArgs = job.JobArgs.Select(a=> new OddJobSerializedParameter(a.Name,a.Value)).ToArray();
            TypeExecutedOn = job.TypeExecutedOn.AssemblyQualifiedName;
            Status = job.Status;
            MethodGenericTypes = job.MethodGenericTypes.Select(q => q.AssemblyQualifiedName).ToArray();
            RetryParameters = retryParameters ?? new RetryParameters();
            ExecutionTime = executionTime;
            QueueName = queueName;
        }
        public Guid JobId { get; set; }
        public OddJobSerializedParameter[] JobArgs { get; set; }
        public string TypeExecutedOn { get; set; }
        public string MethodName { get; set; }
        public string Status { get; set; }
        public string[] MethodGenericTypes { get; set; }
        public RetryParameters RetryParameters { get; set; }
        public DateTimeOffset? ExecutionTime { get; set; }
        public string QueueName { get; set; }
    }
}