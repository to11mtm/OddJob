using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using Newtonsoft.Json;

namespace OddJob.Rpc.MagicOnion.PerfSampl
{
    public class MockAdder: ISerializedJobQueueAdder
    {
        public void AddJob(SerializableOddJob jobData)
        {
            string obj = "";
            try
            {
                obj = JsonConvert.SerializeObject(jobData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            //Console.WriteLine(obj);
        }

        public Task AddJobAsync(SerializableOddJob jobData,
            CancellationToken cancellationToken = default)
        {
            AddJob(jobData);
            return Task.CompletedTask;
        }

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            throw new NotImplementedException();
        }

        public Task AddJobsAsync(IEnumerable<SerializableOddJob> jobDataSet,
            CancellationToken cancellationToken = default)
        {
            AddJobs(jobDataSet);
            return Task.CompletedTask;
        }
    }
}