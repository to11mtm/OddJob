using System.Collections.Concurrent;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class DIShellMockJob
    {
        public static ConcurrentDictionary<string,int> MyCounter= new ConcurrentDictionary<string,int>();
        public void DoThing(string testName, int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter.AddOrUpdate(testName, (a) => 1, (a, b) => b + 1);
        }
    }
}