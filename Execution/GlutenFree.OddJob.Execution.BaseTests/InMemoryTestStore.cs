using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.BaseTests
{
    public class InMemoryTestStore : IJobQueueAdder, IJobQueueManager
    {
        public static ConcurrentDictionary<string, List<OddJobWithMetaAndStorageData>> jobPeeker {get {return jobStore; } }
        private static ConcurrentDictionary<string, List<OddJobWithMetaAndStorageData>> jobStore = new ConcurrentDictionary<string, List<OddJobWithMetaAndStorageData>>();
        private static object jobLock = new object();
        private static object dictionaryLock = new object();
        public Guid AddJob<TJob>(Expression<Action<TJob>> jobExpression, RetryParameters retryParameters = null, DateTimeOffset? executionTime = null, string queueName = "default")
        {
            var jobInfo = JobCreator.Create<TJob>(jobExpression);
            var newJob = new OddJobWithMetaAndStorageData()
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
                    jobStore.GetOrAdd(queueName, new List<OddJobWithMetaAndStorageData>());
                }
            }
            lock (jobLock)
            {
                jobStore[queueName].Add(newJob);
            }
            return newJob.JobId;
        }

        public Task<Guid> AddJobAsync<TJob>(Expression<Action<TJob>> jobExpression,
            RetryParameters retryParameters = null,
            DateTimeOffset? executionTime = null, string queueName = "default",
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Guid>(AddJob(jobExpression,retryParameters,executionTime,queueName));
        }

        public IEnumerable<IOddJobWithMetadata> GetJobs(string[] queueNames, int fetchSize, Expression<Func<JobLockData,object>> orderPredicate)
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
        public void WriteJobState(Guid jobId, Action<OddJobWithMetaAndStorageData> transformFunc)
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

        public IOddJobWithMetadata GetJob(Guid jobId, bool mustLock = true, bool requireValidState = true)
        {
            return jobStore.FirstOrDefault(q => q.Value.Any(r => r.JobId == jobId)).Value
                .FirstOrDefault(q => q.JobId == jobId);
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
            return Task.FromResult(GetJobs(queueNames, fetchSize,
                orderPredicate));
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

        public Task<IOddJobWithMetadata> GetJobAsync(Guid jobId, bool needLock = true, bool requireValidStatus = true, CancellationToken cancellationToken=default)
        {
            return Task.FromResult(GetJob(jobId, needLock));
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
