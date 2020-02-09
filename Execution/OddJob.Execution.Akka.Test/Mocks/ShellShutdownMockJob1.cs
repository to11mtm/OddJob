using System.Collections.Concurrent;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class ShellShutdownMockJob1
    {
        internal static ConcurrentDictionary<int, int> MyCounter = new ConcurrentDictionary<int, int>();
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(derp, 1, ((k, v) => v + 1));
        }
    }
}