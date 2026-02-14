namespace GlutenFree.OddJob.Execution.Akka
{
    public class PluginRecoveryRequest
    {
        public PluginRecoveryRequest(object recoveryMessage)
        {
            RecoveryMessage = recoveryMessage;
        }
        public object RecoveryMessage { get; protected set; }
    }
}