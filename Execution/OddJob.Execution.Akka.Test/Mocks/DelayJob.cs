using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class DelayJob
    {
        public static ConcurrentDictionary<string,int> MsgCounter = new ConcurrentDictionary<string, int>();
        public async Task DoDelay(string msg)
        {
            MsgCounter.AddOrUpdate(msg, (m) => 1, (m, i) => i + 1);
            await Task.Delay(TimeSpan.FromSeconds(1));
            //SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
        }
    }
}