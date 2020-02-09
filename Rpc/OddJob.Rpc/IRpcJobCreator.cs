using System;
using System.Collections.Generic;
using GlutenFree.OddJob.Serializable;
using MagicOnion;

namespace OddJob.RpcServer
{
    public interface IRpcJobCreator : IService<IRpcJobCreator>
    {
        /// <summary>
        /// Adds a Job to the Queue.
        /// </summary>
        /// <param name="jobData">The Serialized Job Data</param>
        /// <returns>If successful, the GUID of the job added.</returns>
        UnaryResult<Guid?> AddJob(SerializableOddJob jobData);

        /// <summary>
        /// Adds a Series of jobs to the queue
        /// </summary>
        /// <param name="jobs">The Serialized job Data</param>
        /// <returns>If successful, the GUID of the job added.</returns>
        UnaryResult<List<Guid>> AddJobs(IEnumerable<SerializableOddJob> jobs);
    }
}