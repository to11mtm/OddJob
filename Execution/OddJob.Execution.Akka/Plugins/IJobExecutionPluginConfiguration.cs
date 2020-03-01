using Akka.Actor;

namespace GlutenFree.OddJob.Execution.Akka
{
    public interface IJobExecutionPluginConfiguration
    {
        Props CreationProps { get; set; }
        object ConfigMsg { get; set; }
    }
}