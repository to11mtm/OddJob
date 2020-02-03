using OddJob.Rpc.Client;

namespace OddJob.Rpc.Execution.Plugin
{
    public class SetStreamingServerConfiguration
    {
        public RpcClientConfiguration Configuration { get; set; }
        public GRPCChannelPool Pool { get; set; }
    }
}