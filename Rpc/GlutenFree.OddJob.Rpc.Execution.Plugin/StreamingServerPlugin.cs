using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Dispatch;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Execution.Akka.Messages;
using OddJob.Rpc.Client;

namespace OddJob.Rpc.Execution.Plugin
{
    public class StreamingServerPlugin : ReceiveActor
    {
        public StreamingServerPlugin()
        {

            Receive<SetStreamingServerConfiguration>(r => SetConfig(r));
            ReceiveAsync<RefreshQueue>(Refresh);
            ReceiveAsync<SetJobQueueConfiguration>(SetQueueAndConnect);
            ReceiveAsync<ShutDownQueues>(StopQueues);
            ReceiveAsync<PluginRecoveryRequest>(PerformRecovery);
        }

        public async Task PerformRecovery(PluginRecoveryRequest request)
        {
            if (request.RecoveryMessage is RecoveryPayload r)
            {
                SetConfig(r.StreamingServerConfiguration);
                await SetQueueAndConnect(r.JobQueueConfiguration);
            }
        }

        /// <summary>
        /// Creates an <see cref="IJobExecutionPluginConfiguration"/> object
        /// to pass into your ActorSystem. 
        /// </summary>
        /// <param name="conf">The RPC Client Configuration</param>
        /// <param name="pool">A GRPC Channel Pool</param>
        /// <param name="secondsBetweenRefresh">Number of seconds between refreshes. This is a fairly lightweight operation so 5-10s is probably a good value.</param>
        /// <param name="secondsToExpire">Sets the expiration time for a keepalive token. This must always be larger than seconds between refreshes.</param>
        /// <returns></returns>
        public static IJobExecutionPluginConfiguration
            CreatePluginConfiguration(
                RpcClientConfiguration conf, GRPCChannelPool pool,
                int secondsBetweenRefresh, int secondsToExpire)
        {
            if (secondsBetweenRefresh > secondsToExpire)
            {
                throw new ArgumentException(
                    $"{nameof(secondsToExpire)} must be greater than or equal to {nameof(secondsBetweenRefresh)}");
            }

            return new StreamingServerPluginConfiguration(
                Props.Create(() => new StreamingServerPlugin()),
                new SetStreamingServerConfiguration()
                {
                    Configuration = conf,
                    Pool = pool,
                    SecondsBetweenRefresh = secondsBetweenRefresh,
                    SecondsTillExpiration = secondsToExpire
                });
        }

        private int NumWorkers = 0;
        private int ShutdownCount = 0;
        private StreamingQueueWorkerClient client;
        private string QueueName;
        private ICancelable loopCancel;
        private IActorRef coordinator;

        private async Task StopQueues(ShutDownQueues q)
        {
            loopCancel.Cancel();
            await client.Stop(QueueName);
            Context.Parent.Tell(new QueueShutDown());
        }

        private SetJobQueueConfiguration _jobQueueConfiguration;
        private SetStreamingServerConfiguration _serverConfiguration;

        private async Task SetQueueAndConnect(SetJobQueueConfiguration jobQueueConfiguration)
        {
            _jobQueueConfiguration = jobQueueConfiguration;
            QueueName = jobQueueConfiguration.QueueName;
            await client.Join(QueueName,
                DateTime.Now.AddSeconds(SecondsBetweenRefresh));

            loopCancel = Context.System.Scheduler
                .ScheduleTellRepeatedlyCancelable(
                    TimeSpan.FromSeconds(SecondsBetweenRefresh),
                    TimeSpan.FromSeconds(SecondsBetweenRefresh),
                    Context.Self, new RefreshQueue(), Self);
        }

        private async Task Refresh(RefreshQueue q)
        {
            await client.Refresh(QueueName,
                DateTime.Now.AddSeconds(SecondsTillExpiration));
        }

        private void SetConfig(SetStreamingServerConfiguration serverConfig)
        {
            _serverConfiguration = serverConfig;
            coordinator = Context.Sender;
            client =
                new StreamingQueueWorkerClient(serverConfig.Pool, serverConfig.Configuration,
                    Context.Parent, Context.Self);
            SecondsBetweenRefresh = serverConfig.SecondsBetweenRefresh;
            SecondsTillExpiration = serverConfig.SecondsTillExpiration;
        }

        public override void AroundPreRestart(Exception cause, object message)
        {
            coordinator.Tell(new PluginRecoveryRequest(
                new RecoveryPayload(_jobQueueConfiguration, _serverConfiguration)));
            base.AroundPreRestart(cause, message);
        }

        public int SecondsBetweenRefresh { get; set; }

        public int SecondsTillExpiration { get; set; }
    }
}