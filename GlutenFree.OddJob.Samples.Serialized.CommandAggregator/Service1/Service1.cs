using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public class Service1
    {
        public static BaseJobExecutorShell CreateAndStartEngine1()
        {
            var container = new SimpleInjector.Container();
            container.Register<IService1Contract,ConsoleWriter>();
            container.Verify();
            var engine = new Execution.Akka.HardInjectedJobExecutorShell(
                () => new JobQueueLayerActor(new SQLiteJobQueueManager(SQLiteSampleHelper.ConnFactoryFunc(),
                    GenerateMappings.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver())),
                () => new JobWorkerActor(new DefaultJobExecutor(new SimpleInjectorContainerFactory(container))),
                () => new JobQueueCoordinator(), new StandardConsoleEngineLoggerConfig("DEBUG"));
            engine.StartJobQueue("console", 20, 3);
            return engine;
        }
    }
}