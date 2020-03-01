using System;
using System.Collections.Generic;
using Grpc.Core;
using MagicOnion.Client;

namespace OddJob.Rpc.Client
{
    public class RpcClientConfiguration : RpcConfiguration
    {
        public RpcClientConfiguration(string host, int port, ChannelCredentials channelCredentials, IEnumerable<IClientFilter> filters,
            IEnumerable<ChannelOption> channelOptions):base(host, port)
        {
            Filters = filters ??
                      throw new ArgumentNullException(nameof(filters));
                        ChannelCredentials = channelCredentials ??
                                 throw new ArgumentNullException(
                                     nameof(channelCredentials));
            ChannelOptions = channelOptions ??
                             throw new ArgumentNullException(
                                 nameof(channelOptions));
        }

        protected bool Equals(RpcClientConfiguration other)
        {
            return Filters.Equals(other.Filters) &&
                   ChannelOptions.Equals(other.ChannelOptions) &&
                   ChannelCredentials.Equals(other.ChannelCredentials) &&
                   Port == other.Port && Host == other.Host;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RpcClientConfiguration) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Filters.GetHashCode();
                hashCode = (hashCode * 397) ^ ChannelOptions.GetHashCode();
                hashCode = (hashCode * 397) ^ ChannelCredentials.GetHashCode();
                hashCode = (hashCode * 397) ^ Port;
                hashCode = (hashCode * 397) ^ Host.GetHashCode();
                return hashCode;
            }
        }
        public ChannelCredentials ChannelCredentials { get; }
        public IEnumerable<ChannelOption> ChannelOptions { get;  }
        public IEnumerable<IClientFilter> Filters { get;  }
        

    }
}