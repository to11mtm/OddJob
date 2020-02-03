using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka;

namespace OddJob.Rpc.Execution.Plugin
{
    public class
        StreamingServerPluginConfiguration : IJobExecutionPluginConfiguration
    {
        public StreamingServerPluginConfiguration(Props creationProps, object configMsg)
        {
            CreationProps = creationProps;
            ConfigMsg = configMsg;
        }

        public Props CreationProps { get; set; }
        public object ConfigMsg { get; set; }
    }
}