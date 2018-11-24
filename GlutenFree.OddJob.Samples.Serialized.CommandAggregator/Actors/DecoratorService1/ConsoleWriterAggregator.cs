using Akka.Actor;
using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An Aggregator that Decorates the command with an additional piece of work.
    /// </summary>
    public class ConsoleWriterAggregator : ActorBase
    {
        protected override bool Receive(object message)
        {
            if (message is MyInProcessCommand)
            {
                var msg = (MyInProcessCommand) message;
                Context.Sender.Tell(new MyInProcessCommand(msg.Counter, msg.ResultingCommands.WithItem(
                    SerializableJobCreator.CreateJobDefiniton((IService1Contract c) =>
                        c.WriteToConsole(string.Format("Hello from {0}", msg.Counter)), queueName:"console"))));

            }

            return true;
        }
    }
}