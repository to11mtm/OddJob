using System;
using System.Threading.Tasks;
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
using MagicOnion;
using MagicOnion.Hosting;
using MagicOnion.HttpGateway.Swagger;
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OddJob.Rpc.Integration.SimpleInjector;
using OddJob.Rpc.MagicOnion.PerfSampl;
using OddJob.RpcServer;
using OddJob.Storage.Sql.SqlServer.Test;
using SimpleInjector;

namespace OddJob.Rpc.WebApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
             var container = new Container();           
            container.Register<IContainerFactory, TestSimpleInjectorContainerFactory>();
            bool useSqlServer = false;
            
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
               var cmd= SQLiteUnitTestTableHelper.heldConnection.CreateCommand();
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
                container.Register<IJobQueueDataConnectionFactory>(()=>new SQLiteJobQueueDataConnectionFactory(SQLiteUnitTestTableHelper.connString));

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
            container.Register<JobQueueCoordinator,JobQueueCoordinator>();
            
            container.Register<RpcJobCreationServer>();
            container.Register<StreamingJobCreationServer>();
            // setup MagicOnion hosting.

            var magicOnionHost = MagicOnionHost.CreateDefaultBuilder()
                .UseMagicOnion(new Type[] {typeof(StreamingJobCreationServer), typeof(RpcJobCreationServer)},
                    new MagicOnionOptions()
                    {
                        MagicOnionServiceActivator =
                            new SimpleInjectorActivator(),
                        ServiceLocator =
                            new SimpleInjectorServiceLocator(container)
                    },
                    new ServerPort("localhost", 12345,
                        ServerCredentials.Insecure))
                .UseConsoleLifetime()
                .Build();

            // NuGet: Microsoft.AspNetCore.Server.Kestrel
            var webHost = new WebHostBuilder()
                .ConfigureServices(collection =>
                {
                    // Add MagicOnionServiceDefinition for reference from Startup.
                    collection.AddSingleton<MagicOnionServiceDefinition>(
                        magicOnionHost.Services
                            .GetService<MagicOnionHostedServiceDefinition>()
                            .ServiceDefinition);
                })
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5432")
                .Build();

            // Run and wait both.
            await Task.WhenAll(webHost.RunAsync(), magicOnionHost.RunAsync());
        }
    }

// WebAPI Startup configuration.
    public class Startup
    {
        // Inject MagicOnionServiceDefinition from DIl
        public void Configure(IApplicationBuilder app,
            MagicOnionServiceDefinition magicOnion)
        {
            // Optional:Add Summary to Swagger
            // var xmlName = "Sandbox.NetCoreServer.xml";
            // var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

            // HttpGateway requires two middlewares.
            // One is SwaggerView(MagicOnionSwaggerMiddleware)
            // One is Http1-JSON to gRPC-MagicOnion gateway(MagicOnionHttpGateway)
            app.UseMagicOnionSwagger(magicOnion.MethodHandlers,
                new SwaggerOptions("MagicOnion.Server",
                    "Swagger Integration Test", "/")
                {
                    // XmlDocumentPath = xmlPath
                });
            app.UseMagicOnionHttpGateway(magicOnion.MethodHandlers,
                new Channel("localhost:12345", ChannelCredentials.Insecure));
        }
    }
}