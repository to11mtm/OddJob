using System;
using System.Collections.Generic;
using GlutenFree.OddJob.Interfaces;
using Grpc.Core;
using MagicOnion.Client;
using MagicOnion.Server;
using OddJob.Rpc;

namespace OddJob.RpcServer
{
    public class RpcServerConfiguration : RpcConfiguration
    {
        public RpcServerConfiguration(string host, int port,
            ServerCredentials credentials,
            IList<MagicOnionServiceFilterDescriptor> globalFilters,
            IServiceLocator serviceLocator, IMagicOnionServiceActivator activator) : base(host, port)
        {
            Credentials = credentials ??
                          throw new ArgumentNullException(nameof(credentials));
            GlobalFilters = globalFilters ??
                            throw new ArgumentNullException(
                                nameof(globalFilters));
            ServiceLocator = serviceLocator ??
                             throw new ArgumentNullException(
                                 nameof(serviceLocator));
            Activator = activator;
        }

        public ServerCredentials Credentials { get; }
        public IList<MagicOnionServiceFilterDescriptor> GlobalFilters { get; }
        public IServiceLocator ServiceLocator { get; set; }
        public IMagicOnionServiceActivator Activator { get; }
    }
}