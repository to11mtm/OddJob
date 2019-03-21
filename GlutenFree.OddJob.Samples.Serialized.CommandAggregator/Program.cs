using System;
using System.Data.SQLite;
using System.Linq;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /*
     * I'm using Akka.NET for this example as it was the easiest to express intent here.
     * This pattern is usable with HTTP or MQ Semantics as well.
     */

    class Program
    {

        private static SQLiteConnection _heldConnection;

        private static BaseJobExecutorShell counterEngine;
        private static BaseJobExecutorShell consoleEngine;

        

        
        static void Main(string[] args)
        {

            SampleTableHelper.EnsureTablesExist(GenerateMappings.TableConfigurations.Select(q => q.Value)
                .Append(new SqlDbJobQueueDefaultTableConfiguration()));
            WriteInstructions();
            /*
             * Execution Engine:
             * Normally each of these would be it's own process, separate from your WebAPI or Akka or MQ Layer.
             * For ease of demonstration, this is rolled up into this single program.
             */
            consoleEngine = Service1.CreateAndStartEngine1();

            counterEngine = Service2.CreateAndStartEngine2();

            /*
             * Service Layer:
             * Normally this would be it's own process, WebAPI, Akka, MQ, or similar layer.
             * For ease of demonstration, this is rolled up into a single program.
             */
            var actorsystem = ActorSystem.Create("sampleAgg");
            var aggRef = actorsystem.ActorOf(Props.Create(() => new Aggregator()),"aggregator");

            /*
             * This is to simulate activity. Consider it a message/request to your service layer.
             */
            actorsystem.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1), aggRef,
                new MyCommand(), null);

            //Quit code.
            while (System.Console.ReadLine().ToLower() != "end")
            {
                WriteInstructions();
            }
            //Cleanly shut down worker engines when done.
            counterEngine.ShutDownQueue("counter");
            consoleEngine.ShutDownQueue("console");
            //Cleanly terminate service layer.
            actorsystem.Terminate().RunSynchronously();
        }

        public static void WriteInstructions()
        {
            System.Console.WriteLine("**********************************************");
            System.Console.WriteLine("TYPE \"end\" (no quotes) and hit enter to exit");
            System.Console.WriteLine("**********************************************");
        }
    }
}
