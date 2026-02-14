using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class ExecuteJobCommand
    {
        public IOddJobWithMetadata Job { get; private set; }

        public ExecuteJobCommand(IOddJobWithMetadata job)
        {
            Job = job;
        }
    }
}