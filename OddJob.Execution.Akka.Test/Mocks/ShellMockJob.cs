namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class ShellMockJob
    {
        internal static int MyCounter = 0;
        public void DoThing(int derp)
        {
            System.Console.WriteLine("I {0}ed", derp);
            MyCounter = MyCounter + 1;
        }
    }
}