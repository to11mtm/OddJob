using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OddJob.RpcServer
{
    public class TimedCache<T> : ITimedCache<T>
    {
        public ConcurrentDictionary<T, DateTime> Items { get; }=
            new ConcurrentDictionary<T, DateTime>();

        public void Freshen(T item, DateTime expiresAt)
        {
            Items.AddOrUpdate(item, (k) => expiresAt, (k, val) => expiresAt);
        }

        public T[] GetItems()
        {
            //Get Unexpired Entries
            var results = Items.Where(r => r.Value >= DateTime.Now)
                .OrderByDescending(r=>r.Value).Select(r => r.Key)
                .ToArray();
            if ((Items.Count - results.Length) > 10)
            {
                //Run cleanup on BG Thread.
                Task.Run(() => Cleanup(Items, cleanupLock));
            }

            return results;
        }
        
        private object cleanupLock = new object();
        private static void Cleanup(ConcurrentDictionary<T, DateTime> container, object lockState)
        {
            lock (lockState)
            {
                //Look for expired entries
                var res = container.Where(r => r.Value < DateTime.Now);
                foreach (var keyValuePair in res)
                {
                    //Try to remove the key
                    DateTime outCheck;
                    var removed =
                        container.TryRemove(keyValuePair.Key, out outCheck);
                    if (removed && outCheck > DateTime.Now)
                    {
                        //If we got here, it got freshened while enumerating.
                        //Put it back in, unless it somehow got freshened again.
                        container.AddOrUpdate(keyValuePair.Key, k => outCheck,
                            (k, dt) => dt > outCheck ? dt : outCheck);
                    }
                }
            }
        }
    }
}