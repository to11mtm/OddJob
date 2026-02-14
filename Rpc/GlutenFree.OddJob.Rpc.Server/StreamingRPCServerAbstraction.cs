using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using OddJob.RpcServer;

namespace GlutenFree.OddJob.Rpc.Server
{
    public class StreamingRPCServerAbstraction<TKeyCacheStore,TCacheStore,TKey>
        where TKeyCacheStore:IKeyedTimedCacheStore<TCacheStore,TKey>
        where TCacheStore:ITimedCache<TKey>
    {
        
        Random r = new Random();
        private ISerializedJobQueueAdder _jobQueueAdder;
        private StreamingJobCreationServerOptions _options;
        public TKeyCacheStore LiveConnections { get; }
        public StreamingRPCServerAbstraction(
            ISerializedJobQueueAdder jobQueueAdder,
            TKeyCacheStore timeCache,
            StreamingJobCreationServerOptions options)
        {
            
            _jobQueueAdder = jobQueueAdder;
            LiveConnections = timeCache;
            _options = options;
        }

        public  async Task AddJobAndNotifySubscribers(SerializableOddJob job, Func<TKey[], SerializableOddJob, Task> groupFunc)
        { 
            await _jobQueueAdder.AddJobAsync(job);
            await NotifyQueueSubscribers(job, groupFunc);
            
        }

        public void JoinMonitoring(TKey connectionId, string queueName,
            DateTime expiresAt)
        {
            
            var connQ =
                LiveConnections.GetOrCreate(queueName);
            connQ.Freshen(connectionId, expiresAt);
        }

        public async Task AddJobsAndNotifySubscribers(
            List<SerializableOddJob> jobDataSet,
            Func<TKey[], SerializableOddJob, Task> groupFunc)
        {
            await _jobQueueAdder.AddJobsAsync(jobDataSet);
            foreach (var job in jobDataSet)
            { 
              await  NotifyQueueSubscribers(job, groupFunc);
            }
        }

        private  async Task NotifyQueueSubscribers(SerializableOddJob jobData, Func<TKey[], SerializableOddJob, Task> groupFunc)
        {
            try
            {


                var connQ = LiveConnections.GetOrCreate(jobData.QueueName);
                var set = connQ.GetItems().ToList();
                if (set.Any())
                {
                    var nd = r.NextDouble();


                    List<TKey> broadcastlist = null;
                    if (connQ.ShouldNotRandomize == false)
                    {
                        broadcastlist = set.Select((id, i) => new {id, i})
                            .Where(
                                vidx =>
                                    (nd > 0.5)
                                        ? vidx.i % 2 > 0
                                        : vidx.i % 2 == 0)
                            .Select(rec => rec.id)
                            .Take(_options.MaxBroadcastToNodes).ToList();
                        if (broadcastlist.Count < _options.MinBroadcastToNodes)
                        {
                            var newSet = set
                                .Where(r => broadcastlist.Contains(r) == false)
                                .ToList();
                            broadcastlist.AddRange(newSet.Take(
                                _options.MinBroadcastToNodes -
                                broadcastlist.Count));

                        }
                    }
                    else
                    {
                        broadcastlist= set.Take(_options.MaxBroadcastToNodes)
                            .ToList();
                    }



                    await groupFunc(broadcastlist.ToArray(), jobData);

                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}