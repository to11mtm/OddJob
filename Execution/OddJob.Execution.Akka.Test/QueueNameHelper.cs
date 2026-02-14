using System;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public static class QueueNameHelper
    {

        public static string CreateQueueName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}