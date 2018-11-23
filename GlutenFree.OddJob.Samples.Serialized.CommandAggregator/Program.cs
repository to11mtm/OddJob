using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Transactions;
using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{

    public class MyTableConfigs : ISqlDbJobQueueTableConfiguration
    {
        public string QueueTableName { get; set;}
        public string ParamTableName { get; set;}
        public int JobClaimLockTimeoutInSeconds { get; set;}
        public string JobMethodGenericParamTableName { get; set;}
    }
    /// <summary>
    /// This is a helper class to provide mappings for Queuenames to specific tables.
    /// If you want to use a single Queue table, you do not need to do this.
    /// But for scenarios where you wish to have multiple queue tables,
    /// This is an opportunity to provide a level of indirection for serialized jobs.
    /// </summary>
    public static class GenerateMappings
    {
        public static Dictionary<string,ISqlDbJobQueueTableConfiguration> TableConfigurations
        {
            get
            {
                return new Dictionary<string, ISqlDbJobQueueTableConfiguration>()
                {
                    {
                        "console",
                        new MyTableConfigs()
                        {
                            QueueTableName = "consoleQueue", ParamTableName = "consoleParam",
                            JobMethodGenericParamTableName = "consoleGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    },
                    { "counter",
                    new MyTableConfigs()
                    {
                        QueueTableName = "counterQueue", ParamTableName = "counterParam",
                        JobMethodGenericParamTableName = "counterGeneric", JobClaimLockTimeoutInSeconds = 30
                    }
                    }
                };
            }
        }
    }
    /*
     * I'm using Akka.NET for this example as it was the easiest to express intent here.
     * This pattern is usable with HTTP or MQ Semantics as well.
     */
    class Program
    {
        internal static readonly string connString = "FullUri=file::memory:?cache=shared";
        private static SQLiteConnection _heldConnection;

        static void Main(string[] args)
        {

            SampleTableHelper.EnsureTablesExist(GenerateMappings.TableConfigurations.Select(q => q.Value)
                .Append(new SqlDbJobQueueDefaultTableConfiguration()));
            WriteInstructions();
            /*
             * Execution Engine:
             * Normally this would be it's own process, separate from your WebAPI or Akka or MQ Layer.
             * For ease of demonstration, this is rolled up into a single program.
             */
            var consoleEngine = new GlutenFree.OddJob.Execution.Akka.HardInjectedJobExecutorShell(
                () => new JobQueueLayerActor(new SQLiteJobQueueManager(ConnFactoryFunc(),
                    GenerateMappings.TableConfigurations["console"],new NullOnMissingTypeJobTypeResolver())),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), new StandardConsoleEngineLoggerConfig("DEBUG"));
            consoleEngine.StartJobQueue("console", 5,5);

            var counterEngine = new GlutenFree.OddJob.Execution.Akka.HardInjectedJobExecutorShell(
                () => new JobQueueLayerActor(new SQLiteJobQueueManager(ConnFactoryFunc(),
                    GenerateMappings.TableConfigurations["counter"], new NullOnMissingTypeJobTypeResolver())),
                () => new JobWorkerActor(new DefaultJobExecutor(new DefaultContainerFactory())),
                () => new JobQueueCoordinator(), new StandardConsoleEngineLoggerConfig("DEBUG"));
            counterEngine.StartJobQueue("counter", 5, 5);

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
            actorsystem.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(3), aggRef,
                new MyCommand(), null);

            //Quit code.
            while (System.Console.ReadLine().ToLower() != "end")
            {
                WriteInstructions();
            }
            counterEngine.ShutDownQueue("default");
            consoleEngine.ShutDownQueue("default");
            actorsystem.Terminate().RunSynchronously();
        }

        public static void WriteInstructions()
        {
            System.Console.WriteLine("**********************************************");
            System.Console.WriteLine("TYPE \"end\" (no quotes) and hit enter to exit");
            System.Console.WriteLine("**********************************************");
        }

        public static SQLiteJobQueueDataConnectionFactory ConnFactoryFunc()
        {
            return new GlutenFree.OddJob.Storage.SQL.SQLite.SQLiteJobQueueDataConnectionFactory(connString);
        }
    }

    /// <summary>
    /// An example job. You could instead define an interface as a contract on the creation end,
    /// allowing for a fully detached implementation.
    /// </summary>
    public class ConsoleWriter
    {
        public void WriteToConsole(string consoleMessage)
        {
            System.Console.WriteLine(consoleMessage);
        }
    }

    /// <summary>
    /// An example job. You could instead define an interface as a contract on the creation end,
    /// allowing for a fully detached implementation.
    /// </summary>
    public class CounterWriter
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

    /// <summary>
    /// A final Command to be sent to the Result Writer.
    /// If you wish to work with Guaranteed delivery semantics,
    /// this could have a Sequence number (if using something like Akka.NET)
    /// Or another ID for your system of choice.
    /// </summary>
    public class AggregatedCommand
    {
        //If you want to work with guaranteed delivery semantics,
        //this would be a good place to put a sequence number for the command writer to use for acknowledging.
        public AggregatedCommand(SerializableOddJob[] resultingCommands)
        {
            ResultingCommands = resultingCommands;
        }
        public SerializableOddJob[] ResultingCommands { get; protected set; }
    }

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

    /// <summary>
    /// A helper class to allow for appending to arrays.
    /// </summary>
    public static class ArrayHelper
    {
        public static T[] WithItem<T>(this T[] array, T newItem)
        {
            return array.Append(newItem).ToArray();
        }
    }

    /// <summary>
    /// A command. While here it does nothing,
    /// in other cases it could contain contextual data to pass to each command decorator.
    /// </summary>
    public class MyCommand
    {

    }

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

    /// <summary>
    /// An Aggregator.
    /// In some ways it could act like a workflow,
    /// Or maybe it's just more of a coordinator between services for one or more commands.
    /// </summary>
    public class Aggregator : ActorBase
    {
        private IActorRef counterAggRef;
        private IActorRef consoleAggRef;
        private IActorRef resultWriterRef;
        private int counter;

        public Aggregator()
        {
            /*
             * Of course, you can use your DI of choice here instead.
             */
            counterAggRef = Context.System.ActorOf(Props.Create(() => new CounterWriterAggregator()), "counterAgg");
            consoleAggRef = Context.System.ActorOf(Props.Create(() => new ConsoleWriterAggregator()), "consoleAgg");
            resultWriterRef =
                Context.System.ActorOf(
                    Props.Create(() =>
                        new ResultJobWriter(new SQLiteJobQueueAdder(
                            new SQLiteJobQueueDataConnectionFactory(Program.connString),
                            new QueueNameBasedJobAdderQueueTableResolver(GenerateMappings.TableConfigurations,
                                new SqlDbJobQueueDefaultTableConfiguration())))),
                    "jobWriter");
            counter = 0;
        }

        protected override bool Receive(object message)
        {
            if (message is MyCommand)
            {
                counter = counter + 1;
                /*
                 * This is far from the only way to implement this pattern. Consider:
                 *  - Sending the original command to each aggregator, and building the result here
                 *  - Pivoting based on command types returned at each stage
                 *  - Having the aggregator decide on the next chain for the pipeline to follow.
                 */

                var building = counterAggRef.Ask(new MyInProcessCommand(counter, new SerializableOddJob[]{})).Result as MyInProcessCommand;
                var building2 = consoleAggRef.Ask(building).Result as MyInProcessCommand;
                var done = resultWriterRef.Ask(new AggregatedCommand(building2.ResultingCommands)).Result as CommandsWritten;
                System.Console.WriteLine("Got Ack for {0}", counter);
            }

            return true;
        }
    }


    /// <summary>
    /// An Aggregator that Decorates the command with an additional piece of work.
    /// </summary>
    public class CounterWriterAggregator : ActorBase
    {
        protected override bool Receive(object message)
        {
            if (message is MyInProcessCommand)
            {
                var msg = (MyInProcessCommand)message;
                Context.Sender.Tell(new MyInProcessCommand(msg.Counter, msg.ResultingCommands.WithItem(
                    SerializableJobCreator.CreateJobDefiniton((CounterWriter c) =>
                            c.WriteCounter<MyParam<string, string>>(
                                new MyParam<string, string> {Param = "genericParam"}),
                        queueName: "counter"))));
            }

            return true;
        }
    }

    public class MyParam<T,TV>
    {
        public T Param { get; set; }
        public TV AnotherParam { get; set; }
    }

    /// <summary>
    /// An Aggregator that Decorates the command with an additional piece of work.
    /// </summary>
    public class ConsoleWriterAggregator : ActorBase
    {
        protected override bool Receive(object message)
        {
            if (message is MyInProcessCommand)
            {
                var msg = (MyInProcessCommand) message;
                Context.Sender.Tell(new MyInProcessCommand(msg.Counter, msg.ResultingCommands.WithItem(
                    SerializableJobCreator.CreateJobDefiniton((ConsoleWriter c) =>
                        c.WriteToConsole(string.Format("Hello from {0}", msg.Counter)), queueName:"console"))));

            }

            return true;
        }
    }

    /// <summary>
    /// This is a writer that will write the jobs to the database in a single transaction,
    /// then acknowledging. This can be extended with guaranteed delivery semantics.
    /// </summary>
    public class ResultJobWriter : ActorBase
    {
        private readonly ISerializedJobQueueAdder _jobQueueAdder;
        public ResultJobWriter(ISerializedJobQueueAdder jobQueueAdder)
        {
            _jobQueueAdder = jobQueueAdder;
        }
        protected override bool Receive(object message)
        {
            if (message is AggregatedCommand)
            {
                var msg = message as AggregatedCommand;
                /*
                 * This pattern is Safe.
                 * If you follow the rules in this example,
                 * Treating your messages as immutable and assembling an aggregated command,
                 * Either every Job is queued, or none are.
                 * Your workers can have their own rules about idempotent processing.
                 *
                 * If you were to use Guaranteed Delivery in something like Akka,
                 * You would want CommandsWritten to send back a sequence number (Sent by AggregatedCommand)
                 * You could also have this pattern writer as part of a WebAPI or MQ solution.
                 */
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    _jobQueueAdder.AddJobs(msg.ResultingCommands);
                    scope.Complete();
                    Context.Sender.Tell(new CommandsWritten());
                }
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
