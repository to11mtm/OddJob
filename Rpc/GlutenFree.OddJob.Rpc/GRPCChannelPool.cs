using System;
using System.Collections.Concurrent;
using System.Linq;
using Grpc.Core;
using MessagePack;

namespace OddJob.Rpc.Client
{
    
    /// <summary>
    /// A Simple pool for GRPC Channels.
    /// This should normally be treated as a Singleton.
    /// </summary>
    public class GRPCChannelPool : IDisposable
    {
        
        private ConcurrentDictionary<RpcClientConfiguration,Channel> _openConnections = new ConcurrentDictionary<RpcClientConfiguration, Channel>();
        private ConcurrentDictionary<RpcClientConfiguration,object> _connLocks = new ConcurrentDictionary<RpcClientConfiguration, object>();
        
        public Channel GetChannel(RpcClientConfiguration conf)
        {
            Channel channel = null;
            // Disable: This format is custom to 'best-fit'
            // when we should and shouldn't lock on access.
            // If anything it's too paranoid.
            // ReSharper disable once InconsistentlySynchronizedField
            _openConnections.TryGetValue(conf, out channel);
            if (channel == null || channel.State == ChannelState.Shutdown)
            {
                object myLock = null;
                if (!_connLocks.TryGetValue(conf, out myLock))
                {
                    _connLocks.TryAdd(conf, new object());
                    _connLocks.TryGetValue(conf, out myLock);
                }


                lock (myLock)
                {
                    
                        _openConnections.TryGetValue(conf, out channel); 
                    if (channel == null || channel.State == ChannelState.Shutdown)
                    {
                        try
                        {
                            channel?.ShutdownAsync().Wait();
                        }
                        catch
                        {
                        }
                        _openConnections.TryAdd(conf, new Channel(conf.Host,
                            conf.Port,
                            conf.ChannelCredentials,
                            conf.ChannelOptions));
                        _openConnections.TryGetValue(conf, out channel);
                    }
                }

            }

            return channel;
        }

        public void Dispose()
        {
            _openConnections.ToList().AsParallel()
                .ForAll((r) => r.Value.ShutdownAsync().Wait());
        }
    }
}