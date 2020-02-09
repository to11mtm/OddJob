using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using Moq;
using Newtonsoft.Json;
using OddJob.Rpc.Client;
using OddJob.Rpc.Integration.SimpleInjector;
using OddJob.RpcServer;
using SimpleInjector;
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
namespace OddJob.Rpc.MagicOnion.PerfSampl
{
    

    public class MockAdder: ISerializedJobQueueAdder
    {
        public void AddJob(SerializableOddJob jobData)
        {
            string obj = "";
            try
            {
                 obj = JsonConvert.SerializeObject(jobData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //Console.WriteLine(obj);
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            throw new NotImplementedException();
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = new Container();
            container.Register<ISerializedJobQueueAdder>(() => new MockAdder(),
                Lifestyle.Singleton);
            container.Register<RpcJobCreationServer>();
            container.Register<StreamingJobCreationServer>();
            container.Verify();
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

            var iters = 150000;
            using (var pool = new GRPCChannelPool())
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
                        await client.AddJobAsync(new SerializableOddJob()
                            {QueueName = "lol"});
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

        private static void RPCSample(Container container)
        {
            var server = RpcJobCreationServiceWrapper.StartService(
                new RpcServerConfiguration("localhost", 9001,
                    ServerCredentials.Insecure,
                    new List<MagicOnionServiceFilterDescriptor>(),
                    new SimpleInjectorServiceLocator(container),
                    new SimpleInjectorActivator()));
            Console.WriteLine("Started...");
            Console.ReadLine();
            var sw = new Stopwatch();

            var iters = 100000;
            using (var pool = new GRPCChannelPool())
            {
                var client = new RPCSerialziedJobQueueAdder(
                    pool,
                    new RpcClientConfiguration("localhost", 9001,
                        ChannelCredentials.Insecure, new IClientFilter[] { },
                        new ChannelOption[] { }));
                sw.Start();
                for (int i = 0; i < iters; i++)
                {
                    client.AddJob(new SerializableOddJob() {QueueName = "lol"});
                }

                sw.Stop();
            }

            Console.WriteLine(
                $"Done... {sw.Elapsed.TotalSeconds} seconds for {iters} iterations");
            Console.ReadLine();
            server.ShutdownAsync().Wait();
        }
    }
}