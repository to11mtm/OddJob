using System;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Serializable
{
    public static class SerializableJobCreator
    {
        /// <summary>
        /// Creates a Serialized Job Definition.
        /// Complex Parameters are stored as JSON strings,
        /// And Type Information is stored in a nonversioned format by default.
        /// </summary>
        /// <typeparam name="T">The type of the job to execute.</typeparam>
        /// <param name="jobExpression">The job Expression</param>
        /// <param name="retryParameters">The retry parameters to use, if any.</param>
        /// <param name="executionTime">A time to schedule; use this to schedule jobs in future</param>
        /// <param name="queueName">The Queue to resolve</param>
        /// <param name="typeNameSerializer">To change the default behavior (nonversioned types) specify a different <see cref="ITypeNameSerializer"/> here.</param>
        /// <returns>A wire-safe Serializable job definition.</returns>
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