namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An Acknowledgement message to signal that a command is written.
    /// If you wish to work with Guaranteed delivery semantics,
    /// this could have a Sequence number (if using something like Akka.NET)
    /// Or another ID for your system of choice.
    /// </summary>
    public class CommandsWritten
    {
        //If you want to work with guaranteed delivery semantics,
        //this would be a good place to put a sequence number to respond back to a delivery buffer.
    
    }
}