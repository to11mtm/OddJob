using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;
using Newtonsoft.Json;

namespace GlutenFree.OddJob.Storage.FileSystem
{
    public class FileSystemJobQueueAdder : IJobQueueAdder
    {
        public FileSystemJobQueueAdder(string fileNameWithPath)
        {
            FileName = fileNameWithPath;
        }

        public string FileName { get; protected set; }

        public void WriteJobToQueue(FileSystemJobMetaData data)
        {
            bool written = false;
            while (!written)
            {
                try
                {
                    using (FileStream fs =
                           File.Open(FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        byte[] toRead = null;
                        using (var memStream = new MemoryStream())
                        {
                            fs.CopyTo(memStream);
                            toRead = memStream.ToArray();
                        }

                        List<FileSystemJobMetaData> serializedData = null;
                        if (toRead.Length > 0)
                        {
                            serializedData =
                                Newtonsoft.Json.JsonConvert.DeserializeObject<List<FileSystemJobMetaData>>(
                                    Encoding.UTF8.GetString(toRead),
                                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                        }
                        else
                        {
                            serializedData = new List<FileSystemJobMetaData>();
                        }

                        serializedData.Add(data);
                        var newData = Newtonsoft.Json.JsonConvert.SerializeObject(serializedData,
                            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                        var toWrite = Encoding.UTF8.GetBytes(newData);
                        fs.Position = 0;
                        fs.SetLength(toWrite.Length);
                        fs.Write(toWrite, 0, toWrite.Length);
                        written = true;
                    }
                }
                catch (IOException ex)
                {
                    //Magic Number Source:
                    //https://stackoverflow.com/questions/2568875/how-to-tell-if-a-caught-ioexception-is-caused-by-the-file-being-used-by-another
                    if (ex.HResult == unchecked((int)0x80070020))
                    {
                        //Retry.
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default")
        {
            var jobData = JobCreator.Create(jobExpression);
            var toSer = new FileSystemJobMetaData()
            {
                JobId = Guid.NewGuid(),
                JobArgs = jobData.JobArgs,
                MethodName = jobData.MethodName,
                RetryParameters = retryParameters ?? new RetryParameters(0, TimeSpan.FromSeconds(0), 0, null),
                TypeExecutedOn = jobData.TypeExecutedOn,
                QueueName = queueName,
                CreatedOn = DateTime.Now,
                Status = JobStates.New,
                MethodGenericTypes = jobData.MethodGenericTypes
            };
            WriteJobToQueue(toSer);
            return toSer.JobId;
        }

        public Task<Guid> AddJobAsync<TJob>(Expression<Action<TJob>> jobExpression,
            RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AddJob(jobExpression, retryParameters,
                executionTime, queueName));
        }

        public Task<Guid> AddJobAsync<TJob>(Expression<Func<TJob, Task>> jobExpression,
            RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default",
            CancellationToken cancellationToken = default)
        {
            return AddJobAsyncInternal<TJob>(jobExpression, retryParameters, executionTime, queueName, cancellationToken);
        }

        public Task<Guid> AddJobAsync<TJob>(Expression<Func<TJob, ValueTask>> jobExpression,
            RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default",
            CancellationToken cancellationToken = default)
        {
            return AddJobAsyncInternal<TJob>(jobExpression, retryParameters, executionTime, queueName, cancellationToken);
        }

        public Task<Guid> AddJobAsync<TJob, T>(Expression<Func<TJob, ValueTask<T>>> jobExpression,
            RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default",
            CancellationToken cancellationToken = default)
        {
            return AddJobAsyncInternal<TJob>(jobExpression, retryParameters, executionTime, queueName, cancellationToken);
        }

// UwU~ Internal helper to avoid code duplication!
        private Task<Guid> AddJobAsyncInternal<TJob>(LambdaExpression jobExpression,
            RetryParameters retryParameters,
            DateTimeOffset? executionTime, string queueName,
            CancellationToken cancellationToken)
        {
            // Ami-chan is using the same logic as AddJob, but async style!
            var jobData = JobCreator.Create<TJob>(jobExpression);
            var toSer = new FileSystemJobMetaData()
            {
                JobId = Guid.NewGuid(),
                JobArgs = jobData.JobArgs,
                MethodName = jobData.MethodName,
                RetryParameters = retryParameters ?? new RetryParameters(0, TimeSpan.FromSeconds(0), 0, null),
                TypeExecutedOn = jobData.TypeExecutedOn,
                QueueName = queueName,
                CreatedOn = DateTime.Now,
                Status = JobStates.New,
                MethodGenericTypes = jobData.MethodGenericTypes
            };
            // File IO is blocking, but for now, we’ll keep it simple and wrap in Task.Run, nyan~
            return Task.Run(() =>
            {
                WriteJobToQueue(toSer);
                return toSer.JobId;
            }, cancellationToken);
        }
    }
}
