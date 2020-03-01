using Akka.Actor;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class JobExecutionResultRecordingPluginConfiguration :
            IJobExecutionPluginConfiguration
    {
        public JobExecutionResultRecordingPluginConfiguration(
            Props creationProps, SetRecordingConfiguration configuration)
        {
            CreationProps = creationProps;
            ConfigMsg = configuration;
        }
        public Props CreationProps { get; set; }
        public object ConfigMsg { get; set; }
    }
}