using Akka.Actor;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Execution.Akka.Messages;
using OddJob.Rpc.Client;

namespace OddJob.Rpc.Execution.Plugin
{
    public class StreamingServerPlugin : ActorBase
    {
        public static IJobExecutionPluginConfiguration PropFactory(RpcClientConfiguration conf, GRPCChannelPool pool)
        {
            return new StreamingServerPluginConfiguration(
                Props.Create(() => new StreamingServerPlugin()),
                new SetStreamingServerConfiguration()
                {
                    Configuration = conf,
                    Pool =  pool
                });
        }

        private int NumWorkers = 0;
        private int ShutdownCount = 0;
        private StreamingQueueWorkerClient client;
        private string QueueName;
        protected override bool Receive(object message)
        {
            if (message is SetStreamingServerConfiguration _r)
            {
                client = new StreamingQueueWorkerClient(_r.Pool, _r.Configuration,Context.Parent);
            }
            else if (message is SetJobQueueConfiguration _s)
            {
                QueueName = _s.QueueName;
                client.Join(QueueName);
            }
            else if (message is ShutDownQueues)
            {
                client.Stop(QueueName);
                Context.Parent.Tell(new QueueShutDown());
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}