using System;
using System.Collections.Generic;
using System.Linq;
using GlutenFree.OddJob.Serializable;
using MagicOnion;
using MagicOnion.Server;

namespace OddJob.RpcServer
{
    public class RpcJobCreationServer :ServiceBase<IRpcJobCreator>, IRpcJobCreator
    {
        private ISerializedJobQueueAdder _jobQueueAdder;

        public RpcJobCreationServer(ISerializedJobQueueAdder jobQueueAdder)
        {
            _jobQueueAdder = jobQueueAdder;
        }

        public UnaryResult<Guid?> AddJob(SerializableOddJob jobData)
        {
            try
            {
                
                _jobQueueAdder.AddJob(jobData);
                
                return new UnaryResult<Guid?>(jobData.JobId);
            }
            catch (Exception e)
            {
            }
            return  new UnaryResult<Guid?>((Guid?)null);
        }

        public UnaryResult<List<Guid>> AddJobs(IEnumerable<SerializableOddJob> jobs)
        {
            try
            {
                _jobQueueAdder.AddJobs(jobs);
                return new UnaryResult<List<Guid>>(jobs.Select(r => r.JobId)
                    .ToList());
            }
            catch (Exception e)
            {
            }
            return new UnaryResult<List<Guid>>(rawValue: null);
        }
    }
}