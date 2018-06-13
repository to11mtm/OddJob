using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;

namespace OddJob.Execution.Akka
{
    public class ShutdownPriorityMailbox : UnboundedPriorityMailbox
    {
        protected ShutdownPriorityMailbox(Settings settings, Config config) : base(settings, config)
        {
        }

        protected override int PriorityGenerator(object message)
        {


            if (message is ShutDownQueues)
            {
                return 0;
            }

            return 1;
        }
    }
}
