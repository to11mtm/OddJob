using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;
using Newtonsoft.Json;

namespace GlutenFree.OddJob.Storage.FileSystem
{
    public class FileSystemJobQueueManager : IJobQueueManager
    {
        public FileSystemJobQueueManager(string fileNameWithPath)
        {
            FileName = fileNameWithPath;
        }

        public string FileName { get; protected set; }


        public IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames,
            int fetchSize, Expression<Func<JobLockData, object>> orderPredicate)
        {
            IEnumerable<IOddJobWithMetadata> results = null;
            bool written = false;
            while (!written)
            {
                try
                {
                    using (FileStream fs =
                        File.Open(FileName, FileMode.Open, FileAccess.ReadWrite,
                            FileShare.None))
                    {
                        byte[] toRead = null;
                        using (var memStream = new MemoryStream())
                        {
                            fs.CopyTo(memStream);
                            toRead = memStream.ToArray();
                        }

                        var serializer =
                            Newtonsoft.Json.JsonConvert
                                .DeserializeObject<List<FileSystemJobMetaData>>(
                                    Encoding.UTF8.GetString(toRead),
                                    new JsonSerializerSettings()
                                    {
                                        TypeNameHandling = TypeNameHandling.All
                                    });
                        var filtered = serializer.Where(q =>
                                queueNames.Contains(q.QueueName)
                                && (q.Status == JobStates.New ||
                                    q.Status == JobStates.Retry)
                                &&
                                ((q.RetryParameters == null ||
                                  q.Status == JobStates.New) ||
                                 (q.RetryParameters.RetryCount <=
                                  q.RetryParameters.MaxRetries)
                                 &&
                                 (q.RetryParameters.LastAttempt == null
                                  || q.RetryParameters.LastAttempt.Value.Add(
                                      q.RetryParameters.MinRetryWait)
                                  < DateTime.Now)))
                            .OrderBy(q =>
                                Math.Min(q.CreatedOn.Ticks,
                                    (q.RetryParameters ?? new RetryParameters(0,
                                         TimeSpan.FromSeconds(0), 0, null))
                                    .LastAttempt
                                    .GetValueOrDefault(DateTime.MaxValue).Ticks)
                            ).Take(fetchSize).ToList();
                        foreach (var item in filtered)
                        {
                            item.Status = "Queued";
                            item.QueueTime = DateTime.Now;
                        }

                        var newData =
                            Newtonsoft.Json.JsonConvert.SerializeObject(
                                serializer,
                                new JsonSerializerSettings()
                                    {TypeNameHandling = TypeNameHandling.All});
                        var toWrite = Encoding.UTF8.GetBytes(newData);
                        fs.Position = 0;
                        fs.SetLength(toWrite.Length);
                        fs.Write(toWrite, 0, toWrite.Length);
                        written = true;
                        results = filtered;
                    }
                }
                catch (IOException ex)
                {
                    //Magic Number Source:
                    //https://stackoverflow.com/questions/2568875/how-to-tell-if-a-caught-ioexception-is-caused-by-the-file-being-used-by-another
                    if (ex.HResult == unchecked((int) 0x80070020))
                    {
                        //Retry.
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return results;
        }

        public IOddJobWithMetadata GetJob(Guid jobId, bool mustLock = true, bool requireValidStatus = true)
        {
            IOddJobWithMetadata results = null;
            bool written = false;
            while (!written)
            {
                try
                {
                    using (FileStream fs =
                        File.Open(FileName, FileMode.Open, FileAccess.ReadWrite,
                            FileShare.None))
                    {
                        byte[] toRead = null;
                        using (var memStream = new MemoryStream())
                        {
                            fs.CopyTo(memStream);
                            toRead = memStream.ToArray();
                        }

                        var serializer =
                            Newtonsoft.Json.JsonConvert
                                .DeserializeObject<List<FileSystemJobMetaData>>(
                                    Encoding.UTF8.GetString(toRead),
                                    new JsonSerializerSettings()
                                    {
                                        TypeNameHandling = TypeNameHandling.All
                                    });
                        var filtered = serializer.Where(q => q.JobId == jobId)
                            .FirstOrDefault();
                        written = true;
                        results = filtered;
                    }
                }
                catch (IOException ex)
                {
                    //Magic Number Source:
                    //https://stackoverflow.com/questions/2568875/how-to-tell-if-a-caught-ioexception-is-caused-by-the-file-being-used-by-another
                    if (ex.HResult == unchecked((int) 0x80070020))
                    {
                        //Retry.
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return results;
        }

        public Task MarkJobSuccessAsync(Guid jobGuid, CancellationToken cancellationToken=default)
        {
            MarkJobSuccess(jobGuid);
            return Task.FromResult(0);
        }

        public Task MarkJobFailedAsync(Guid jobGuid, CancellationToken cancellationToken=default)
        {
            MarkJobFailed(jobGuid);
            return Task.FromResult(0);
        }

        public Task<IEnumerable<IOddJobWithMetadata>> GetJobsAsync(string[] queueNames, int fetchSize,
            Expression<Func<JobLockData, object>> orderPredicate, CancellationToken cancellationToken=default)
        {
            return Task.FromResult<IEnumerable<IOddJobWithMetadata>>(GetJobs(queueNames,fetchSize,orderPredicate));
        }

        public Task MarkJobInProgressAsync(Guid jobId, CancellationToken cancellationToken=default)
        {
            MarkJobInProgress(jobId);
            return Task.FromResult(0);
        }

        public Task MarkJobInRetryAndIncrementAsync(Guid jobId, DateTime lastAttempt, CancellationToken cancellationToken=default)
        {
            MarkJobInRetryAndIncrement(jobId,lastAttempt);
            return Task.FromResult(0);
        }

        public Task<IOddJobWithMetadata> GetJobAsync(Guid jobId, bool needLock = true, bool requireValidStatus =true, CancellationToken cancellationToken=default)
        {
            return Task.FromResult(GetJob(jobId, needLock));
        }

        public void WriteJobState(Guid jobId,
            Action<FileSystemJobMetaData> transformFunc)
        {
            bool written = false;
            while (!written)
            {
                try
                {
                    using (FileStream fs =
                        File.Open(FileName, FileMode.Open, FileAccess.ReadWrite,
                            FileShare.None))
                    {
                        byte[] toRead = null;
                        using (var memStream = new MemoryStream())
                        {
                            fs.CopyTo(memStream);
                            toRead = memStream.ToArray();
                        }

                        var serializer =
                            Newtonsoft.Json.JsonConvert
                                .DeserializeObject<List<FileSystemJobMetaData>>(
                                    Encoding.UTF8.GetString(toRead),
                                    new JsonSerializerSettings()
                                    {
                                        TypeNameHandling = TypeNameHandling.All
                                    });
                        var filtered = serializer.FirstOrDefault(q =>
                            q.JobId == jobId);
                        if (filtered != null)
                        {
                            transformFunc(filtered);
                        }

                        var newData =
                            Newtonsoft.Json.JsonConvert.SerializeObject(
                                serializer,
                                new JsonSerializerSettings()
                                    {TypeNameHandling = TypeNameHandling.All});

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
                    if (ex.HResult == unchecked((int) 0x80070020))
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


        public void MarkJobFailed(Guid jobGuid)
        {
            WriteJobState(jobGuid, (q) =>
            {
                q.Status = JobStates.Failed;
                q.FailureTime = DateTime.Now;
            });
        }

        public void MarkJobInProgress(Guid jobId)
        {
            WriteJobState(jobId, (q) =>
            {
                q.Status = "InProgress";
                q.LastAttemptTime = DateTime.Now;
            });
        }

        public void MarkJobInRetryAndIncrement(Guid jobId, DateTime lastAttempt)
        {
            WriteJobState(jobId, (q) =>
            {
                q.Status = JobStates.Retry;
                q.RetryParameters.LastAttempt = lastAttempt;
                q.RetryParameters.RetryCount = q.RetryParameters.RetryCount + 1;
            });
        }

        public void MarkJobSuccess(Guid jobGuid)
        {
            WriteJobState(jobGuid, (q) =>
            {
                q.Status = JobStates.Processed;
                q.LastAttemptTime = DateTime.Now;
            });
        }
    }
}
