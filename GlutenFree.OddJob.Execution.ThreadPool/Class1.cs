using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.ThreadPool
{

    public class ThreadPoolQueueFiller
    {
        private readonly IJobQueueManager _queueManager;
        private readonly Timer _timer;
        private readonly ConcurrentQueue<IOddJobWithMetadata> _queue;
        private readonly string _queueName;
        private readonly int _fetchSize;
        private readonly CancellationToken _cancelToken;
        private readonly ThreadPoolCoordinator _coordinator;
        private readonly IJobStateExtension _jobStateExtension;

        public int SaturationPulseCount { get; protected set; }
        public DateTime? SaturationStartTime { get; protected set; }
        public int QueueLifeSaturationPulseCount { get; protected set; }
        public ThreadPoolQueueFiller(IJobQueueManager queueManager, ConcurrentQueue<IOddJobWithMetadata> queue, IJobStateExtension stateExtension,string queueName, int fetchSize, TimeSpan interval, CancellationToken cancelToken, ThreadPoolCoordinator coordinator)
        {
            _queueManager = queueManager;
            _queueName = queueName;
            _fetchSize = fetchSize;
            _cancelToken = cancelToken;
            _jobStateExtension = stateExtension;
            _timer = new Timer((state)=>PulseQueue(), null,(int)(interval.TotalMilliseconds/2), (int)interval.TotalMilliseconds);
        }

        /// <summary>
        /// Paranoia: If we are somehow thread starved we want to avoid multiple pulses.
        /// </summary>
        private object _lockObject = new object();

        public void ShutdownQueue()
        {
            _timer.Dispose();
        }

        public void PulseQueue()
        {
            if (_cancelToken.IsCancellationRequested == false)
            {
                if (_coordinator.PendingItems <= (_fetchSize * 2))
                {
                    //In case we somehow have a thread starvation condition,
                    //We prevent multi-reads by locking and double-checking Here
                    lock (_lockObject)
                    {
                        if (_coordinator.PendingItems <= (_fetchSize * 2))
                        {
                            SaturationStartTime = null;
                            SaturationPulseCount = 0;
                            var jobs = _queueManager.GetJobs(new[] {_queueName}, _fetchSize);
                            foreach (var oddJobWithMetadata in jobs)
                            {
                                _queue.Enqueue(oddJobWithMetadata);
                                _coordinator.IncreasePendingItems();
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        lock (_lockObject)
                        {
                            SaturationStartTime = SaturationStartTime ?? DateTime.Now;
                            SaturationPulseCount = SaturationPulseCount + 1;
                            QueueLifeSaturationPulseCount = QueueLifeSaturationPulseCount + 1;
                            _jobStateExtension.OnJobQueueSaturated(DateTime.Now, SaturationPulseCount,
                                QueueLifeSaturationPulseCount);
                        }
                    }
                    catch (Exception e)
                    {
                        //TODO: Better Logging.
                    }
                }
            }
            else
            {
                ShutdownQueue();
            }
        }
    }
    public class ThreadPoolCoordinator
    {
        public string QueueName { get; protected set; }
        public bool ShuttingDown { get; protected set; }
        public int WorkerCount { get; protected set; }

        private int _pendingItems;
        public int PendingItems
        {
            get { return _pendingItems; }
        }
        public TimeSpan PulseTime { get; protected set; }
        private readonly CancellationTokenSource tokenSource;
        private readonly List<ThreadPoolExecutor> threadPoolExecutors;
        private readonly ConcurrentQueue<IOddJobWithMetadata> jobQueue;
        private readonly ThreadPoolQueueFiller _queueFiller;
        public ThreadPoolCoordinator(string queueName, int workerCount, TimeSpan pulseTime, Func<IJobExecutor> jobExecutorFunc, Func<IJobQueueManager> jobQueueManagerFunc,Func<IJobStateExtension> jobStateExtensionFunc)
        {
            QueueName = queueName;
            ShuttingDown = false;
            WorkerCount = workerCount;
            _pendingItems = 0;
            PulseTime = pulseTime;
            tokenSource = new CancellationTokenSource();
            threadPoolExecutors = new List<ThreadPoolExecutor>(WorkerCount);
            jobQueue = new ConcurrentQueue<IOddJobWithMetadata>();
            _queueFiller= new ThreadPoolQueueFiller(jobQueueManagerFunc(), jobQueue,jobStateExtensionFunc(), queueName, workerCount, pulseTime, tokenSource.Token, this);
            
            for (int i = 0; i < workerCount; i++)
            {
                var executor = new ThreadPoolExecutor(tokenSource.Token, jobQueue, jobExecutorFunc(), new ThreadPoolJobStateHandler(jobQueueManagerFunc(), jobStateExtensionFunc()), this);
                threadPoolExecutors.Add(executor);
                System.Threading.ThreadPool.QueueUserWorkItem((obj) => executor.StartLoop());
            }
        }

        public void DecreasePendingItems()
        {
            Interlocked.Decrement(ref _pendingItems);
        }

        public void IncreasePendingItems()
        {
            Interlocked.Increment(ref _pendingItems);
        }

        public void ShutdownQueue(TimeSpan timeout)
        {
            _queueFiller.ShutdownQueue();
            SpinWait.SpinUntil(() => _pendingItems == 0, timeout);
        }

    }

    public interface IJobStateExtension
    {
        void OnJobFailed(IOddJobWithMetadata jobWithMetadata);
        void OnJobSuccess(IOddJobWithMetadata jobWithMetadata);
        void OnJobRetry(IOddJobWithMetadata jobWithMetadata);
        void OnJobQueueSaturated(DateTime now, int saturationPulseCount, int queueLifeSaturationPulseCount);
    }

    public class BaseJobStateExtension : IJobStateExtension
    {
        public virtual void OnJobFailed(IOddJobWithMetadata jobWithMetadata)
        {
            
        }

        public virtual void OnJobSuccess(IOddJobWithMetadata jobWithMetadata)
        {
            
        }

        public virtual void OnJobRetry(IOddJobWithMetadata jobWithMetadata)
        {
            
        }

        public virtual void OnJobQueueSaturated(DateTime now, int saturationPulseCount, int queueLifeSaturationPulseCount)
        {
            
        }
    }

    public class ThreadPoolJobStateHandler
    {
        private readonly IJobQueueManager _jobQueueManager;
        private readonly IJobStateExtension _jobStateExtension;
        public ThreadPoolJobStateHandler(IJobQueueManager jobQueueManager, IJobStateExtension jobStateExtension)
        {
            _jobQueueManager = jobQueueManager;
            _jobStateExtension = jobStateExtension;
        }
        public void HandleSuccess(IOddJobWithMetadata jobWithMetadata)
        {
            _jobQueueManager.MarkJobSuccess(jobWithMetadata.JobId);
            try
            {
                _jobStateExtension.OnJobSuccess(jobWithMetadata);
            }
            catch (Exception e)
            {
                //TODO: Better Logging
            }
        }

        public void HandleFailure(IOddJobWithMetadata jobWithMetadata, Exception exception)
        {

            if (jobWithMetadata.RetryParameters == null || jobWithMetadata.RetryParameters.MaxRetries <= jobWithMetadata.RetryParameters.RetryCount)
            {
                _jobQueueManager.MarkJobFailed(jobWithMetadata.JobId);

                try
                {
                    _jobStateExtension.OnJobFailed(jobWithMetadata);
                }
                catch (Exception ex)
                {
                    //TODO: better logging.
                }
            }
            else
            {
                _jobQueueManager.MarkJobInRetryAndIncrement(jobWithMetadata.JobId, DateTime.Now);
                try
                {
                    _jobStateExtension.OnJobRetry(jobWithMetadata);
                }
                catch (Exception ex)
                {
                    //TODO: Better logging;
                }
            }
        }

    }

    public class ThreadPoolExecutor
    {
        private CancellationToken _token;
        private ConcurrentQueue<IOddJobWithMetadata> _jobQueue;
        private readonly IJobExecutor _jobExecutor;
        private ThreadPoolJobStateHandler _jobStateHandler;
        private ThreadPoolCoordinator _coordinator;
        public ThreadPoolExecutor(CancellationToken token, ConcurrentQueue<IOddJobWithMetadata> jobQueue, IJobExecutor jobExecutor, ThreadPoolJobStateHandler jobStateHandler, ThreadPoolCoordinator coordinator)
        {
            _token = token;
            _jobQueue = jobQueue;
            _jobExecutor = jobExecutor;
            _coordinator = coordinator;
        }
        public void StartLoop()
        {
            while (_token.IsCancellationRequested == false)
            {
                IOddJobWithMetadata jobWithMetadata;
                while (_jobQueue.TryDequeue(out jobWithMetadata))
                {
                    try
                    {
                        _jobExecutor.ExecuteJob(jobWithMetadata);
                        _jobStateHandler.HandleSuccess(jobWithMetadata);
                    }
                    catch (Exception e)
                    {
                        _jobStateHandler.HandleFailure(jobWithMetadata, e);
                    }

                    _coordinator.DecreasePendingItems();
                }
            }
        }
    }
}
