using System.Transactions;
using Akka.Actor;
using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// This is a writer that will write the jobs to the database in a single transaction,
    /// then acknowledging. This can be extended with guaranteed delivery semantics.
    /// </summary>
    public class ResultJobWriter : ActorBase
    {
        private readonly ISerializedJobQueueAdder _jobQueueAdder;
        public ResultJobWriter(ISerializedJobQueueAdder jobQueueAdder)
        {
            _jobQueueAdder = jobQueueAdder;
        }
        protected override bool Receive(object message)
        {
            if (message is AggregatedCommand)
            {
                var msg = message as AggregatedCommand;
                /*
                 * This pattern is Safe.
                 * If you follow the rules in this example,
                 * Treating your messages as immutable and assembling an aggregated command,
                 * Either every Job is queued, or none are.
                 * Your workers can have their own rules about idempotent processing.
                 *
                 * If you were to use Guaranteed Delivery in something like Akka,
                 * You would want CommandsWritten to send back a sequence number (Sent by AggregatedCommand)
                 * You could also have this pattern writer as part of a WebAPI or MQ solution.
                 */
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    _jobQueueAdder.AddJobs(msg.ResultingCommands);
                    scope.Complete();
                    Context.Sender.Tell(new CommandsWritten());
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}