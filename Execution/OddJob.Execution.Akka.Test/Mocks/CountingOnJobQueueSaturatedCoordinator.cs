using System;
using System.Collections.Concurrent;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class CountingOnJobQueueSaturatedCoordinator : JobQueueCoordinator
    {
        public static ConcurrentDictionary<string,int> pulseCount = new ConcurrentDictionary<string, int>();
        protected override void OnJobQueueSaturated(DateTime saturationTime, int saturationMissedPulseCount, long queueLifeSaturationPulseCount)
        {
            pulseCount.AddOrUpdate(QueueName, (qn) => 1, (qn, i) => i + 1);
        }
    }
}