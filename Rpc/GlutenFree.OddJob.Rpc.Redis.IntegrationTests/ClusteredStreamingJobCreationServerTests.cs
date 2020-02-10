using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Rpc.Server;
using GlutenFree.OddJob.Serializable;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using Moq;
using OddJob.Rpc.Client;
using OddJob.Rpc.Integration.SimpleInjector;
using OddJob.Rpc.Server.Redis;
using OddJob.RpcServer;
using SimpleInjector;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Oddjob.Rpc.Redis.IntegrationTests
{
    public class ClusteredStreamingJobCreationServerTests : IDisposable
    {
        private const int _redisPort = 9904;
        private RedisInside.Redis redis;
        public void Dispose()
        {
            redis?.Dispose();
            
        }

        public ClusteredStreamingJobCreationServerTests(ITestOutputHelper outputHelper)
        {
            redis = new RedisInside.Redis(c => { c.Port(_redisPort); });
        }
        [Fact]
        public async Task RedisCache_Shares_Infos()
        {
            var mock = new Mock<ISerializedJobQueueAdder>();
            var container = new Container();
            container.Register(()=> mock.Object);
            container.Register<StreamingJobCreationServer>();
            container.Register(() =>
                ConnectionMultiplexer.Connect(new ConfigurationOptions()
                {
                    EndPoints = {{"localhost", _redisPort}}
                }), Lifestyle.Singleton);
            
            container.Verify();
            var server1 = StreamingServiceWrapper.StartService<ClusteredStreamingJobCreationServer>(
                new RpcServerConfiguration("localhost", 9001,
                    ServerCredentials.Insecure,
                    new List<MagicOnionServiceFilterDescriptor>(),
                    new SimpleInjectorServiceLocator(container),
                    new SimpleInjectorActivator()));
            var server2 = StreamingServiceWrapper.StartService<ClusteredStreamingJobCreationServer>(
                new RpcServerConfiguration("localhost", 9002,
                    ServerCredentials.Insecure,
                    new List<MagicOnionServiceFilterDescriptor>(),
                    new SimpleInjectorServiceLocator(container),
                    new SimpleInjectorActivator()));
            var pool = new GRPCChannelPool();
            var mockClient1 = new MockWorkerClient(pool,
                new RpcClientConfiguration("localhost", 9001,
                    ChannelCredentials.Insecure, new IClientFilter[]{},
                    new ChannelOption[]{}));
            var mockClient2 = new MockWorkerClient(pool,
                new RpcClientConfiguration("localhost", 9002,
                    ChannelCredentials.Insecure, new IClientFilter[]{},
                    new ChannelOption[]{}));
            await mockClient1.Join("hello", DateTime.Now.AddMinutes(5));
            
            await mockClient2.Join("hello", DateTime.Now.AddMinutes(5));

            await mockClient1.SendCreatedToServer("123","hello");
            await mockClient2.SendCreatedToServer("456","hello");

            SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
            Assert.True(MockWorkerClient.count.ContainsKey("123"));
            Assert.True(MockWorkerClient.count.ContainsKey("456"));
        }
    }
}