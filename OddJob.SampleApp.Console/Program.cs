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
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())), loggerTypeFactory: null);
            jobQueue.StartJobQueue("sample",5,5);


            var timer = StartSampleJobTimer(store);

            System.Console.WriteLine("Press ENTER to quit...");
            System.Console.ReadLine();
        }

        private static Timer StartSampleJobTimer(InMemoryTestStore store)
        {
            return new Timer(

                (state) =>
                {
                    var counter = CounterClass.Counter;
                    CounterClass.Counter = counter + 1;
                    var counterString = counter.ToString();
                    store.AddJob((SampleJob job) => job.WriteSomething(counterString, DateTime.Now.ToString()), queueName: "sample");
                }, null,
                TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }
    }
    public static class CounterClass
    {
    public static int Counter { get; set; }
    }

    public class SampleJob
    {
        public void WriteSomething(string thing, string time)
        {
            System.Console.WriteLine("Hello! I got {0} at {1}, and am executing at {2}", thing,time,DateTime.Now.ToString());
        }
    }

}
