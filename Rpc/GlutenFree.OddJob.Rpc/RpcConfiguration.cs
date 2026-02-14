using System;
using System.Collections.Generic;
using Grpc.Core;

namespace OddJob.Rpc
{
    public class RpcConfiguration
    {
        public RpcConfiguration(string host, int port)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
        }

        public string Host { get; }
        public int Port { get;}

    }
}