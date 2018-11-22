using System;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Serializable
{
    public static class SerializableJobCreator
    {
        public static SerializableOddJob CreateJobDefiniton<T>(Expression<Action<T>> jobExpression,
            RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            return new SerializableOddJob(JobCreator.Create(jobExpression), retryParameters, executionTime, queueName);
        }

        public static IOddJobWithMetadata GetExecutableJobDefinition(SerializableOddJob jobData, IJobTypeResolver jobTypeResolver=null)
        {
            var _jobTypeResolver = jobTypeResolver ?? new NullOnMissingTypeJobTypeResolver();
            return new OddJobWithMetaData()
            {
                ExecutionTime = jobData.ExecutionTime,
                JobArgs = jobData.JobArgs.Select(q =>
                    new OddJobParameter()
                    {
                        Name = q.Name,
                        Value = Newtonsoft.Json.JsonConvert.DeserializeObject(q.Value, Type.GetType(q.TypeName))
                    }).ToArray(),
                JobId = jobData.JobId,
                MethodGenericTypes = jobData.MethodGenericTypes
                    .Select(q => _jobTypeResolver.GetTypeForJob(q)).ToArray(),
                MethodName = jobData.MethodName,
                Queue = jobData.QueueName,
                RetryParameters = jobData.RetryParameters,
                Status = jobData.Status,
                TypeExecutedOn = _jobTypeResolver.GetTypeForJob(jobData.TypeExecutedOn)
            };

        }
    }
}