namespace GlutenFree.OddJob.Manager.Blazor.Models
{
    public class JobTimelineRequest
    {
        public string QueueName { get; set; }
    }

    public class JobTimelineResult
    {
        public List<JobTimelinePoint> Points { get; set; } = new List<JobTimelinePoint>();
    }

    public class JobTimelinePoint
    {
        public string TimeLabel { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; } = new Dictionary<string, int>();
    }
}
