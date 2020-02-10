using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.DI.SimpleInjector;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using GlutenFree.OddJob;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Execution.Akka.Test;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.SQLite.Test;
using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.Sql.TableHelper;
using Grpc.Core;
using LinqToDB.DataProvider.SqlServer;
using MagicOnion.Client;
using MagicOnion.Server;
using OddJob.Rpc.Client;
using OddJob.Rpc.Execution.Plugin;
using OddJob.Rpc.Integration.SimpleInjector;
using OddJob.RpcServer;
using OddJob.Storage.Sql.SqlServer.Test;
using SimpleInjector;
using Xunit;
using Xunit.Abstractions;

namespace OddJob.Rpc.IntegrationTests
{
    public class IntegrationTest
    {
        private ITestOutputHelper Console;
        public IntegrationTest(ITestOutputHelper output)
        {
            Console = output;
        }
        [Fact]
        async Task Main()
        {
            var container = new Container();           
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            bool useSqlServer = true;
            
            if (useSqlServer)
            {
                var cs = SqlConnectionHelper.CheckConnString("rpcsample", "f:\\", false);
                SqlServerUnitTestTableHelper.EnsureTablesExist("rpcsample", "f:\\");
                using (var conn =
                    SqlConnectionHelper.GetLocalDB("rpcsample", "f:\\", false))
                {
                    var cmd = conn.CreateCommand();
                    var tableHelper = new SqlTableHelper(
                        new SqlServerDataConnectionFactory(
                            new TestDbConnectionFactory("f:\\", "rpcsample"),
                            SqlServerVersion.v2008),
                        new SqlServer2008Generator());
                    tableHelper
                        .GetCreateMainTableIndexes(
                            new SqlDbJobQueueDefaultTableConfiguration())
                        .ForEach(t =>
                        {
                            try
                            { 
                                cmd.CommandText = t;
                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                            }
                        });
                    try
                    {
                        cmd.CommandText =
                            tableHelper.CreateParamIndexes(
                                new SqlDbJobQueueDefaultTableConfiguration());
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                    }

                    try
                    {
                        cmd.CommandText= tableHelper.CreateQueueParamIndexes(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                    }
                        
                }

                container.Register<IJobQueueDbConnectionFactory>(()=>new TestDbConnectionFactory("f:\\", "rpcsample"));
                container.Register<IJobQueueDataConnectionFactory>(() =>
                    new SqlServerDataConnectionFactory(
                        container.GetInstance<IJobQueueDbConnectionFactory>(),
                        SqlServerVersion.v2008));
            }
            else
            {
                SQLiteUnitTestTableHelper.EnsureTablesExist();
                var cmd = AkkaTestUnitTestTableHelper.heldConnection.OpenAndReturn().CreateCommand();
                var tableHelper = new SqlTableHelper(
                    new SQLiteJobQueueDataConnectionFactory(SQLiteUnitTestTableHelper
                        .connString), new SQLiteGenerator());
                tableHelper
                    .GetCreateMainTableIndexes(
                        new SqlDbJobQueueDefaultTableConfiguration()).ForEach(t =>
                    {
                        cmd.CommandText = t;
                        cmd.ExecuteNonQuery();
                    });
                container.Register<IJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString));

            }
            
            container.Register<IJobQueueManager, BaseSqlJobQueueManager>();
            container.Register<IJobQueueAdder, BaseSqlJobQueueAdder>();
            
            container.Register<ISerializedJobQueueAdder, BaseSqlJobQueueAdder>();
            container.Register<ISqlDbJobQueueTableConfiguration, SqlDbJobQueueDefaultTableConfiguration>();
            container.Register<IJobAdderQueueTableResolver, DefaultJobAdderQueueTableResolver>();
            container.Register<IJobTypeResolver, NullOnMissingTypeJobTypeResolver>();
            container.Register<IJobExecutor, DefaultJobExecutor>();
            container.Register<JobQueueLayerActor>();
            container.Register<JobWorkerActor>();
            container.Register<JobQueueCoordinator,OverriddenJobCoordinator>();
            
            container.Register<RpcJobCreationServer>();
            container.Register<StreamingJobCreationServer>();
            await StreamingSample(container, 2000,5);
            //RPCSample(container);
        }
        
        private  async Task StreamingSample(Container container, int iters, int numClients)
        {
            var server = StreamingServiceWrapper.StartService<StreamingJobCreationServer>(
                new RpcServerConfiguration("localhost", 9001,
                    ServerCredentials.Insecure,
                    new List<MagicOnionServiceFilterDescriptor>(),
                    new SimpleInjectorServiceLocator(container),
                    new SimpleInjectorActivator()));
            Console.WriteLine("Started...");
            //Console.ReadLine();
            var sw = new Stopwatch();
            var jobServer = new DependencyInjectedJobExecutorShell(ac=> new SimpleInjectorDependencyResolver(container,ac),null );
            var pool = new GRPCChannelPool();
            jobServer.StartJobQueue("default", 50, 60,
                plugins: new IJobExecutionPluginConfiguration[]
                {
                    StreamingServerPlugin.CreatePluginConfiguration(
                        new RpcClientConfiguration("localhost", 9001,
                            ChannelCredentials.Insecure,
                            new IClientFilter[] { }, new ChannelOption[] { }), pool, 2, 30)
                });
            Console.WriteLine("Server Started...");
            //Console.ReadLine();
            {
                Func<StreamingQueueClient> clientFactory = ()=>new StreamingQueueClient(
                    pool,
                    new RpcClientConfiguration("localhost", 9001,
                        ChannelCredentials.Insecure, new IClientFilter[] { },
                        new ChannelOption[] { }));
                Console.WriteLine("Client Started");
                StreamingQueueClient[] clients = new StreamingQueueClient[numClients];
                for (int i = 0; i < numClients; i++)
                {
                    clients[i] = clientFactory();
                }
                Console.WriteLine($"{numClients} Client(s) Started");
                

                Console.WriteLine("Doing Warmup");
                await clients.ForEachAsync(async clt =>
                {
                    await clt.AddJobAsync(
                        SerializableJobCreator.CreateJobDefinition(
                            (SampleJob s) =>
                                s.DoThing(DateTime.Now.ToString())));
                }, numClients);
                Console.WriteLine("Ready to run...");
                
                sw.Start();
                for (int i = 0; i < iters; i++)
                {
                        
                    await clients.ForEachAsync(async clt =>
                    {
                        var now = DateTime.Now.ToString();
                        await clt.AddJobAsync(
                            SerializableJobCreator.CreateJobDefinition(
                                (SampleJob s) =>
                                    s.DoThing(now)));
                    }, numClients);
                       
                }

                var elapsed = sw.Elapsed.TotalSeconds;
                
                
                //Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(
                    $"Done... {sw.Elapsed.TotalSeconds} seconds for {iters} iterations");

                await clients.ForEachAsync(async clt => await clt.CloseAsync(),
                    numClients);
                
                Console.WriteLine($"Clients Closed... took {sw.Elapsed.TotalSeconds-elapsed} seconds to close");
            }

            //Console.ReadLine();
            try
            {
                jobServer.ShutDownQueue("default",5);
            }
            catch 
            {
                //intentional - don't want to wait for large workloads.
            }
            var sw3 =  new Stopwatch();
            sw3.Start();
            server.ShutdownAsync().Wait();
            Console.WriteLine(
                $"Took {sw3.Elapsed.TotalSeconds} to shutdown server");
            Assert.True(SampleJob.setter["lol"]>0);
        }
    }
}