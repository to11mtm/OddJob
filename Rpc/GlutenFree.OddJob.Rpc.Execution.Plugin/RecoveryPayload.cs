using GlutenFree.OddJob.Execution.Akka.Messages;

namespace OddJob.Rpc.Execution.Plugin
{
    public class RecoveryPayload
    {
        public RecoveryPayload(SetJobQueueConfiguration jobQueueConfiguration,
            SetStreamingServerConfiguration streamingServerConfiguration)
        {
            JobQueueConfiguration = jobQueueConfiguration;
            StreamingServerConfiguration = streamingServerConfiguration;
        }

        public SetJobQueueConfiguration JobQueueConfiguration { get; }

        public SetStreamingServerConfiguration StreamingServerConfiguration
        {
            get;
        }
    }
}