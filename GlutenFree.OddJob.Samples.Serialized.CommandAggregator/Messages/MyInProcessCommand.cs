using GlutenFree.OddJob.Serializable;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An In process command.
    /// In this example there is simply a counter and a set of resulting commands.
    /// But you could have other contextual data that is passed to each of the commands,
    /// (or both via envelopes to encapsulate) to aggregate data instead.
    /// </summary>
    public class MyInProcessCommand 
    {
        public MyInProcessCommand(long counter, SerializableOddJob[] resultingCommands)
        {
            Counter = counter;
            ResultingCommands = resultingCommands;
        }
        public long Counter { get; protected set; }
        public SerializableOddJob[] ResultingCommands { get; protected set; }
    }
}