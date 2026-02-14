using Akka.Actor;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An Aggregator.
    /// In some ways it could act like a workflow,
    /// Or maybe it's just more of a coordinator between services for one or more commands.
    /// </summary>
    public class Aggregator : ActorBase
    {
        private IActorRef counterAggRef;
        private IActorRef consoleAggRef;
        private IActorRef resultWriterRef;
        private int counter;

        public Aggregator()
        {
            /*
             * Of course, you can use your DI of choice here instead.
             */
            counterAggRef = Context.System.ActorOf(Props.Create(() => new CounterWriterAggregator()), "counterAgg");
            consoleAggRef = Context.System.ActorOf(Props.Create(() => new ConsoleWriterAggregator()), "consoleAgg");
            resultWriterRef =
                Context.System.ActorOf(
                    Props.Create(() =>
                        new ResultJobWriter(new SQLiteJobQueueAdder(
                            new SQLiteJobQueueDataConnectionFactory(SampleTableHelper.connString),
                            new QueueNameBasedJobAdderQueueTableResolver(GenerateMappings.TableConfigurations,
                                new SqlDbJobQueueDefaultTableConfiguration())))),
                    "jobWriter");
            counter = 0;
        }

        protected override bool Receive(object message)
        {
            if (message is MyCommand)
            {
                counter = counter + 1;
                /*
                 * This is far from the only way to implement this pattern. Consider:
                 *  - Sending the original command to each aggregator, and building the result here
                 *  - Pivoting based on command types returned at each stage
                 *  - Having the aggregator decide on the next chain for the pipeline to follow.
                 *
                 * This is also a great place to use concepts like Guaranteed delivery,
                 * so long as each part of the built request does not have side effects
                 * (i.e. the Jobs are the side effects, and this pattern allows them to be written atomically).
                 */

                var building = counterAggRef.Ask(new MyInProcessCommand(counter, new SerializableOddJob[]{})).Result as MyInProcessCommand;
                var building2 = consoleAggRef.Ask(building).Result as MyInProcessCommand;
                var done = resultWriterRef.Ask(new AggregatedCommand(building2.ResultingCommands)).Result as CommandsWritten;
                System.Console.WriteLine("Got Ack for {0}", counter);
            }

            return true;
        }
    }
}