using System;
using System.Collections.Generic;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class JobSweepResponse
    {
        public JobSweepResponse(IEnumerable<IOddJobWithMetadata> jobs, Guid msgSweepGuid)
        {
            Jobs = jobs;
            SweepGuid = msgSweepGuid;
        }

        public IEnumerable<IOddJobWithMetadata> Jobs { get; set; }

        public Guid SweepGuid { get; set; }
    }
}