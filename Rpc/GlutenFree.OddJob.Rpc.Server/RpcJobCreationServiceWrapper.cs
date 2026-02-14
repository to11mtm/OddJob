using System;
using Grpc.Core;
using MagicOnion.Server;
using OddJob.Rpc;

namespace GlutenFree.OddJob.Rpc.Server
{
    public static class RpcJobCreationServiceWrapper
    {
        public static Grpc.Core.Server StartService(RpcServerConfiguration conf)
        {
            var opts = new MagicOnionOptions();
            opts.GlobalFilters = conf.GlobalFilters;
            opts.ServiceLocator = conf.ServiceLocator;
            opts.SerializerOptions = UnregisteredSerializerOptions.Instance;
            opts.MagicOnionServiceActivator = conf.Activator;
            var svc =MagicOnionEngine.BuildServerServiceDefinition(
                new Type[] {typeof(RpcJobCreationServer)}, opts);
            var server = new Grpc.Core.Server()
            {
                Services = {svc},
                Ports = {new ServerPort(conf.Host, conf.Port, conf.Credentials)}
            };
            server.Start();
            return server;
        }
    }
}