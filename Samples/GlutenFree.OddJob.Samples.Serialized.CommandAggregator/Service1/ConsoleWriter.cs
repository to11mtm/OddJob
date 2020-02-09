namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An example job. You could instead define an interface as a contract on the creation end,
    /// allowing for a fully detached implementation.
    /// </summary>
    public class ConsoleWriter : IService1Contract
    {
        public void WriteToConsole(string consoleMessage)
        {
            System.Console.WriteLine(consoleMessage);
        }
    }
}