using Akka.Actor;
using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An Aggregator that Decorates the command with an additional piece of work.
    /// </summary>
    public class CounterWriterAggregator : ActorBase
    {
        protected override bool Receive(object message)
        {
            if (message is MyInProcessCommand)
            {
                var msg = (MyInProcessCommand)message;
                Context.Sender.Tell(new MyInProcessCommand(msg.Counter, msg.ResultingCommands.WithItem(
                    SerializableJobCreator.CreateJobDefinition((IService2Contract c) =>
                            c.WriteCounter<MyParam<string, string>>(
                                new MyParam<string, string> {Param = "genericParam"}),
                        queueName: "counter"))));
            }

            return true;
        }
    }
}