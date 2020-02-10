using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OddJob.Rpc.Server.Redis;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Oddjob.Rpc.Redis.IntegrationTests
{
    public class RedisHubTimedCacheTests : IDisposable
    {
        private RedisInside.Redis redis;

        public RedisHubTimedCacheTests(ITestOutputHelper outputHelper)
        {
            redis = new RedisInside.Redis(c => { c.Port(9901); });
        }
        [Fact]
        public async Task RedisHubTimedCache_Gets_Events_on_Queue()
        {
            //using (var redis = new RedisInside.Redis(c => { c.Port(9901); }))
            {
                var testqName = 
                    nameof(RedisHubTimedCache_Gets_Events_on_Queue);
                
                var c2 = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions()
                    {
                        ClientName = "hub", EndPoints = {{"localhost", 9901}}
                    });
                var hub = new RedisHubTimedCache(c2,testqName);
                var toTest = StreamingConstants.RedisString + testqName;
                var testGuid = Guid.NewGuid();

                var getPrimary = GetAnyMaster(c2);
                
                await PingAsync(c2, getPrimary, hub.Subscriber).ForAwait();
                var res = c2.GetSubscriber().Publish(new RedisChannel(toTest, RedisChannel.PatternMode.Literal),
                    hub.GetPayload(testGuid, DateTime.Now.AddMinutes(5)));
                SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
                Assert.True(hub.GetItems().Any());
            }
        }
        
        [Fact]
        public async Task RedisHubTimedCache_Sends_Recieves_Queue_On_freshen()
        {
            //using (var redis = new RedisInside.Redis(c => { c.Port(9901); }))
            {
                var testqName = 
                    nameof(RedisHubTimedCache_Sends_Recieves_Queue_On_freshen);
                
                var c2 = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions()
                    {
                        ClientName = "hub", EndPoints = {{"localhost", 9901}}
                    });
                var hub = new RedisHubTimedCache(c2,testqName);
                
                var hub2 = new RedisHubTimedCache(c2,testqName);

                var getPrimary = GetAnyMaster(c2);
                
                await PingAsync(c2, getPrimary, hub.Subscriber).ForAwait();
                
                await PingAsync(c2, getPrimary, hub2.Subscriber).ForAwait();
                hub2.Freshen(Guid.NewGuid(), DateTime.Now.AddMinutes(5));
                //var res = c2.GetSubscriber().Publish(new RedisChannel(toTest, RedisChannel.PatternMode.Literal),
                //hub.GetPayload(testGuid, DateTime.Now.AddMinutes(5)));
                SpinWait.SpinUntil(() => false, TimeSpan.FromSeconds(5));
                Assert.True(hub.GetItems().Any());
            }
        }
        protected IServer GetAnyMaster(IConnectionMultiplexer muxer)
        {
            foreach (var endpoint in muxer.GetEndPoints())
            {
                var server = muxer.GetServer(endpoint);
                if (!server.IsSlave) return server;
            }
            throw new InvalidOperationException("Requires a master endpoint (found none)");
        }
        private static async Task PingAsync(IConnectionMultiplexer muxer, IServer pub, ISubscriber sub, int times = 1)
        {
            while (times-- > 0)
            {
                // both use async because we want to drain the completion managers, and the only
                // way to prove that is to use TPL objects
                var t1 = sub.PingAsync();
                var t2 = pub.PingAsync();
                await Task.Delay(100).ForAwait(); // especially useful when testing any-order mode

                if (!Task.WaitAll(new[] { t1, t2 }, muxer.TimeoutMilliseconds * 2)) throw new TimeoutException();
            }
        }

        public void Dispose()
        {
            redis?.Dispose();
        }
    }
}