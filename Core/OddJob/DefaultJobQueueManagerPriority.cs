using System;
using System.Linq.Expressions;

namespace GlutenFree.OddJob.Interfaces
{
    public static class DefaultJobQueueManagerPriority
    {
        public static Expression<Func<JobLockData, object>> Expression
        {
            get { return data => data.MostRecentDate; }
        }
    }
}