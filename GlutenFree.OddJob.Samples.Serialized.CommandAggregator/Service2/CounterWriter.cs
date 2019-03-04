namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// An example job. You could instead define an interface as a contract on the creation end,
    /// allowing for a fully detached implementation.
    /// </summary>
    public class CounterWriter : IService2Contract
    {
        private static object _lockObject = new object();
        private static int _counter = 0;
        public void WriteCounter<T>(T param)
        {
            lock (_lockObject)
            {
                _counter = _counter + 1;
                System.Console.WriteLine("Counter" + _counter + " Param: " + param.ToString());
            }
        }
    }
}