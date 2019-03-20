using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public class Service2
    {
        public static BaseJobExecutorShell CreateAndStartEngine2()
        {
            var container = new SimpleInjector.Container();
            container.Register<IService2Contract,CounterWriter>();
            container.Verify();
            var engine = new GlutenFree.OddJob.Execution.Akka.HardInjectedJobExecutorShell<JobQueueLayerActor, JobWorkerActor, JobQueueCoordinator>(
                () => new JobQueueLayerActor(new SQLiteJobQueueManager(SQLiteSampleHelper.ConnFactoryFunc(),
                    GenerateMappings.TableConfigurations["counter"], new NullOnMissingTypeJobTypeResolver())),
                () => new JobWorkerActor(new DefaultJobExecutor(new SimpleInjectorContainerFactory(container))),
                () => new JobQueueCoordinator(), new StandardConsoleEngineLoggerConfig("DEBUG"));
            engine.StartJobQueue("counter", 20, 3);
            return engine;
        }
    }
}