using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;
using GlutenFree.OddJob.Execution.Akka.Messages;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class ShutdownPriorityMailbox : UnboundedPriorityMailbox
    {
        public ShutdownPriorityMailbox(Settings settings, Config config) : base(settings, config)
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
