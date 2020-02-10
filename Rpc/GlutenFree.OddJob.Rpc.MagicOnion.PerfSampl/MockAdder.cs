using System;
using System.Collections.Generic;
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

        public void AddJobs(IEnumerable<SerializableOddJob> jobDataSet)
        {
            throw new NotImplementedException();
        }
    }
}