using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobFilterMetadata
    {
        DateTime? MostRecentDate { get; set; }
        DateTime? CreatedDate { get; set; }
        DateTime? LastAttempt { get; set; }
        int Retries { get; set; }
        DateTimeOffset? DoNotExecuteBefore { get; set; }
        string Status { get; set; }
    }
}