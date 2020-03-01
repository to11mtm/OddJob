using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class DelayJob
    {
        public static ConcurrentDictionary<string,int> MsgCounter = new ConcurrentDictionary<string, int>();
        public void DoDelay(string msg)
        {
            MsgCounter.AddOrUpdate(msg, (m) => 1, (m, i) => i + 1);
            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(1));
        }
    }
}