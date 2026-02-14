using System;
using System.Collections.Generic;
using System.Linq;
using GlutenFree.OddJob.Serializable;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using OddJob.RpcServer;

namespace GlutenFree.OddJob.Rpc.Server
{
    public class RpcJobCreationServer :ServiceBase<IRpcJobCreator>, IRpcJobCreator
    {
        private ISerializedJobQueueAdder _jobQueueAdder;

        public RpcJobCreationServer(ISerializedJobQueueAdder jobQueueAdder)
        {
            _jobQueueAdder = jobQueueAdder;
        }

        public async UnaryResult<Guid?> AddJob(SerializableOddJob jobData)
        {
            try
            {
                
                await _jobQueueAdder.AddJobAsync(jobData);
                
                return jobData.JobId;
            }
            catch (Exception e)
            {
                return await ReturnStatus<Guid?>(StatusCode.Internal, e.ToString());
            }  
        }

        public UnaryResult<List<Guid>> AddJobs(
            IEnumerable<SerializableOddJob> jobs)
        {
            try
            {
                _jobQueueAdder.AddJobs(jobs);

                return new UnaryResult<List<Guid>>(jobs.Select(r => r.JobId)
                    .ToList());
            }
            catch (Exception e)
            {
                return ReturnStatus<List<Guid>>(StatusCode.Internal,
                    e.ToString());
            }
        }
    }
}