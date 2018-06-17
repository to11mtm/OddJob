using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OddJob.Execution.Akka;
using OddJob.Execution.Akka.Test;

namespace OddJob.SampleApp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = new InMemoryTestStore();
            var jobQueue = new HardInjectedJobExecutorShell(() => new JobQueueLayerActor(store),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())));
            jobQueue.StartJobQueue("sample",5,5);
            var done = false;
            var timer = new Timer(

                (state) =>
                {
                    var counter = CounterClass.Counter;
                    CounterClass.Counter = counter + 1;
                    var counterString = counter.ToString();
                    store.AddJob((SampleJob job) => job.WriteSomething(counterString), queueName: "sample");
                }, null,
                TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
                System.Console.WriteLine("Press ENTER to quit...");
                System.Console.ReadLine();
        }
    }
    public static class CounterClass
    {
    public static int Counter { get; set; }
    }

    public class SampleJob
    {
        public void WriteSomething(string thing)
        {
            System.Console.WriteLine("Hello! I got {0}", thing);
        }
    }

}
