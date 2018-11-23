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
            RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default", ITypeNameSerializer typeNameSerializer=null)
        {
            var job = JobCreator.Create(jobExpression);
            var mySer = typeNameSerializer ?? new UnversionedTypeSerializer();
            return new SerializableOddJob()
            {
                JobId = job.JobId,
                MethodName = job.MethodName,
                JobArgs = job.JobArgs.Select(a => new OddJobSerializedParameter()
                {
                    Name = a.Name,
                    Value = Newtonsoft.Json.JsonConvert.SerializeObject(a.Value),
                    TypeName = mySer.GetTypeName(a.Value.GetType())
                }).ToArray(),
                TypeExecutedOn = mySer.GetTypeName(job.TypeExecutedOn),
                Status = job.Status,
                MethodGenericTypes = job.MethodGenericTypes.Select(q => mySer.GetTypeName(q)).ToArray(),
                RetryParameters = retryParameters ?? new RetryParameters(),
                ExecutionTime = executionTime,
                QueueName = queueName

            };
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