using System;
using System.Collections.Generic;
using GlutenFree.OddJob.Serializable;
using MagicOnion;

namespace OddJob.RpcServer
{
    public interface IRpcJobCreator : IService<IRpcJobCreator>
    {
        UnaryResult<Guid?> AddJob(SerializableOddJob jobData);
        UnaryResult<List<Guid>> AddJobs(IEnumerable<SerializableOddJob> jobs);
    }
}