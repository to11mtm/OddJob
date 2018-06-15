namespace OddJob.Execution.Akka
{
    public class GetJobs
    {
        public string QueueName { get; protected set; }
        public int FetchSize { get; protected set; }
        public GetJobs(string queueName,int fetchSize)
        {
            QueueName = QueueName;
            FetchSize = fetchSize;
        }
    }
}

