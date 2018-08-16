using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class InMemoryTestStore : IJobQueueAdder, IJobQueueManager
    {
        internal static Dictionary<string, List<OddJobWithMetaData>> jobPeeker {get {return jobStore; } }
        private static Dictionary<string, List<OddJobWithMetaData>> jobStore = new Dictionary<string, List<OddJobWithMetaData>>();
        private static object jobLock = new object();
        private static object dictionaryLock = new object();
        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            var jobInfo = JobCreator.Create<TJob>(jobExpression);
            var newJob = new OddJobWithMetaData()
            {
                JobId = Guid.NewGuid(),
                JobArgs = jobInfo.JobArgs,
                MethodName = jobInfo.MethodName,
                RetryParameters = retryParameters,
                TypeExecutedOn = jobInfo.TypeExecutedOn,
                CreatedOn = DateTime.Now,
                Status = JobStates.New
            };
            if (jobStore.ContainsKey(queueName) == false)
            {
                lock (dictionaryLock)
                {
                    if (jobStore.ContainsKey(queueName) == false)
                    {
                        jobStore.Add(queueName, new List<OddJobWithMetaData>());
                    }
                }
            }
            lock (jobLock)
            {
                jobStore[queueName].Add(newJob);
            }
            return newJob.JobId;
        }

        public IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize)
        {
            List<IOddJobWithMetadata> results = new List<IOddJobWithMetadata>();
            lock (jobLock)
            {
                foreach (var queueName in queueNames)
                {
                    if (jobStore.ContainsKey(queueName))
                    {
                        var filtered = jobStore[queueName].Where(q =>
                        (q.Status == JobStates.New || q.Status == JobStates.Retry)
                        &&
                        ((q.RetryParameters == null) ||
                        (q.RetryParameters.RetryCount < q.RetryParameters.MaxRetries)
                        &&
                        (q.RetryParameters.LastAttempt == null
                          || q.RetryParameters.
                              LastAttempt.Value.Add(q.RetryParameters.MinRetryWait)
                               > DateTime.Now)))
                               .OrderBy(q =>
                               Math.Min(q.CreatedOn.Ticks, (q.RetryParameters ?? new RetryParameters(0, TimeSpan.FromSeconds(0), 0, null)).LastAttempt.GetValueOrDefault(DateTime.MaxValue).Ticks)
                               ).Take(fetchSize).ToList();
                        foreach (var item in filtered)
                        {
                            item.Status = "Queued";
                            item.QueueTime = DateTime.Now;
                        }
                        results.AddRange(filtered);
                    }
                }
            }
            return results;
        }
        public void WriteJobState(Guid jobId, Action<OddJobWithMetaData> transformFunc)
        {
            lock (jobLock)
            {


                var filtered = jobStore.FirstOrDefault(q =>
                q.Value.Any(r => r.JobId == jobId)).Value.FirstOrDefault(s => s.JobId == jobId);
                if (filtered != null)
                {
                    transformFunc(filtered);
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
                q.Status = "In-Progress";
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

        public IOddJobWithMetadata GetJob(Guid jobId)
        {
            return jobStore.FirstOrDefault(q => q.Value.Any(r => r.JobId == jobId)).Value
                .FirstOrDefault(q => q.JobId == jobId);
        }

        public void MarkJobSuccess(Guid jobGuid)
        {
            WriteJobState(jobGuid, (q) =>
            {
                q.Status = "Success";
                q.LastAttemptTime = DateTime.Now;
            });
        }
    }
}
