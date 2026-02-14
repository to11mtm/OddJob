using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class OverriddenJobCoordinator : JobQueueCoordinator
    {
        protected override void OnJobSuccess(JobSuceeded msg)
        {
            Succeeded = Succeeded + 1;
        }

        public static int Succeeded;
    }
}