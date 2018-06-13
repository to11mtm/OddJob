namespace OddJob.Execution.Akka
{
    public class GetJobs
    {
        public string QueueName { get; protected set; }
        public GetJobs(string queueName)
        {
            QueueName = QueueName;
        }
    }
}

