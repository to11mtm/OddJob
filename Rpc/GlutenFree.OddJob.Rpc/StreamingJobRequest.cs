using System;
using MessagePack;

namespace OddJob.Rpc
{
    [MessagePackObject]
    public class StreamingJobRequest
    {
        [Key(0)]
        public int DelayMs { get; set; }
        [Key(1)]
        public string QueueName { get; set; }
        [Key(2)]
        public Guid JobId { get; set; }
        [Key(3)]
        public string MethodName { get; set; }
    }
}