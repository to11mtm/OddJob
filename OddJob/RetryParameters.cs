﻿using System;

namespace OddJob
{
    public class RetryParameters : IRetryParameters
    {
        public int RetryCount { get; }
        public DateTime? LastAttempt { get; }

        public int MaxRetries { get; set; }
        public TimeSpan MinRetryWait { get; set; }

        public RetryParameters(int maxRetries, TimeSpan minRetryWait)
        {
            MaxRetries = maxRetries;
            MinRetryWait = minRetryWait;
        }

        public RetryParameters(int maxRetries, TimeSpan minRetryWait, int retryCount, DateTime? lastAttempt) : this(maxRetries, minRetryWait)
        {
            this.RetryCount = retryCount;
            this.LastAttempt = lastAttempt;
        }
    }
}