using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddJob
{
    public static class JobStates
    {
        public const string Processed = "Processed";
        public const string New = "New";
        public const string Failed = "Failed";
        public const string Retry = "Retry";
        public const string InProgress = "InProgress";
    }
}
