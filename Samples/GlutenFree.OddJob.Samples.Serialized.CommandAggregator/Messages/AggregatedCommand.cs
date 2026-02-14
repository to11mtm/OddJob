using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// A final Command to be sent to the Result Writer.
    /// If you wish to work with Guaranteed delivery semantics,
    /// this could have a Sequence number (if using something like Akka.NET)
    /// Or another ID for your system of choice.
    /// </summary>
    public class AggregatedCommand
    {
        //If you want to work with guaranteed delivery semantics,
        //this would be a good place to put a sequence number for the command writer to use for acknowledging.
        public AggregatedCommand(SerializableOddJob[] resultingCommands)
        {
            ResultingCommands = resultingCommands;
        }
        public SerializableOddJob[] ResultingCommands { get; protected set; }
    }
}