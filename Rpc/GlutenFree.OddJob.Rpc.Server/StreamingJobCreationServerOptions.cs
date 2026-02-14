using System;

namespace GlutenFree.OddJob.Rpc.Server
{
    public class StreamingJobCreationServerOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minBroadcastTo">Min nodes to broadcast to. 2-3 is probably a good number in most cases.</param>
        /// <param name="maxBroadcastTo">Max nodes to broadcast to. 3-4 is probably a good number in most cases.</param>
        /// <exception cref="ArgumentException"></exception>
        public StreamingJobCreationServerOptions(int minBroadcastTo, int maxBroadcastTo)
        {
            if (minBroadcastTo < 0)
            {
                throw new ArgumentException(
                    $"{nameof(minBroadcastTo)} must be greater than zero (0)!",
                    nameof(minBroadcastTo));
            }
            if (maxBroadcastTo < 0)
            {
                throw new ArgumentException(
                    $"{nameof(maxBroadcastTo)} must be greater than zero (0)!",
                    nameof(maxBroadcastTo));
            }
            MinBroadcastToNodes = minBroadcastTo;
            MaxBroadcastToNodes = maxBroadcastTo;
        }

        public int MaxBroadcastToNodes { get; set; }

        public int MinBroadcastToNodes { get; set; }
    }
}