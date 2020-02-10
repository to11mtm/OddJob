﻿using System;
using Grpc.Core;
using MagicOnion.Server;
using OddJob.Rpc;

namespace OddJob.RpcServer
{
    public static class StreamingServiceWrapper
    {
        public static Server StartService<T>(RpcServerConfiguration conf) where T: BaseStreamingJobCreationServer
        {
            var svcDef = GetServiceDefinition<T>(conf);
            var server = new Grpc.Core.Server()
            {
                Services = {svcDef},
                Ports = {new ServerPort(conf.Host, conf.Port, conf.Credentials)}
            };

            server.Start();
            return server;
        }

        public static MagicOnionServiceDefinition GetServiceDefinition<T>(RpcServerConfiguration conf) where T: BaseStreamingJobCreationServer
        {
            var opts = new MagicOnionOptions();
            opts.GlobalFilters = conf.GlobalFilters;
            opts.ServiceLocator = conf.ServiceLocator;
            opts.SerializerOptions = UnregisteredSerializerOptions.Instance;
            opts.MagicOnionServiceActivator = conf.Activator;
            var svc =MagicOnionEngine.BuildServerServiceDefinition(
                new Type[] { typeof(T)}, opts);
            return svc;
        }
    }
}