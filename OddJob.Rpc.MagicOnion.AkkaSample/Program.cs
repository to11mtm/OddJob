using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.DI.SimpleInjector;
using FluentMigrator.Runner.Generators.SQLite;
using GlutenFree.OddJob;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Execution.Akka.Test;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.TableHelper;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using OddJob.Rpc.Client;
using OddJob.Rpc.Execution.Plugin;
using OddJob.Rpc.MagicOnion.PerfSampl;
using OddJob.RpcServer;
using SimpleInjector;

namespace OddJob.Rpc.MagicOnion.AkkaSample
{
    class Program
    {

        static async Task Main(string[] args)
        {
            UnitTestTableHelper.EnsureTablesExist();
            var cmd = UnitTestTableHelper.heldConnection.CreateCommand();
            var tableHelper = new SqlTableHelper(
                new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper
                    .connString), new SQLiteGenerator());
            tableHelper
                .GetCreateMainTableIndexes(
                    new SqlDbJobQueueDefaultTableConfiguration()).ForEach(t =>
                {
                    cmd.CommandText = t;
                    cmd.ExecuteNonQuery();
                });
            var container = new Container();           container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<IJobQueueManager, SQLiteJobQueueManager>();
            container.Register<IJobQueueAdder, SQLiteJobQueueAdder>();
            
            container.Register<ISerializedJobQueueAdder, SQLiteJobQueueAdder>();
            container.Register<SQLiteJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString));
            container.Register<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
            container.Register<IJobExecutor, DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            container.Register<JobQueueCoordinator,OverriddenJobCoordinator>();
            
            container.Register<RpcJobCreationServer>();
            container.Register<StreamingJobCreationServer>();
            await StreamingSample(container);
            //RPCSample(container);
        }
        
        private static async Task StreamingSample(Container container)
        {
            var server = StreamingServiceWrapper.StartService(
                new RpcServerConfiguration("localhost", 9001,
                    ServerCredentials.Insecure,
                    new List<MagicOnionServiceFilterDescriptor>(),
                    new SimpleInjectorServiceLocator(container),
                    new SimpleInjectorActivator()));
            Console.WriteLine("Started...");
            Console.ReadLine();
            var sw = new Stopwatch();

            var iters = 1500;
            var jobServer = new DependencyInjectedJobExecutorShell(ac=> new SimpleInjectorDependencyResolver(container,ac),null );
            var pool = new GRPCChannelPool();
            jobServer.StartJobQueue("default", 10, 5,
                plugins: new IJobExecutionPluginConfiguration[]
                {
                    StreamingServerPlugin.PropFactory(
                        new RpcClientConfiguration("localhost", 9001,
                            ChannelCredentials.Insecure,
                            new IClientFilter[] { }, new ChannelOption[] { }), pool)
                });
            {
                var client = new StreamingQueueClient(
                    pool,
                    new RpcClientConfiguration("localhost", 9001,
                        ChannelCredentials.Insecure, new IClientFilter[] { },
                        new ChannelOption[] { }));
                {
                    sw.Start();
                    for (int i = 0; i < iters; i++)
                    {
                        var now = DateTime.Now.ToString();
                        await client.AddJobAsync(SerializableJobCreator.CreateJobDefinition(
                            (SampleJob s)=> s.DoThing(now))
                            );
                    }

                    sw.Stop();
                }
                await client.CloseAsync();
            }

            Console.WriteLine(
                $"Done... {sw.Elapsed.TotalSeconds} seconds for {iters} iterations");
            Console.ReadLine();
            server.ShutdownAsync().Wait();
        }
    }

    public class SampleJob
    {
        public void DoThing(string thing)
        {
            Console.WriteLine(thing);
        }
    }
}