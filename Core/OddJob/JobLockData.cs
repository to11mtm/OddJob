using System;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public class JobLockData : IJobFilterMetadata
    {
        public long JobId { get; set; }
        public DateTime? MostRecentDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastAttempt { get; set; }
        public int Retries { get; set; }
        public DateTimeOffset? DoNotExecuteBefore { get; set; }
        public string Status { get; set; }
    }
}