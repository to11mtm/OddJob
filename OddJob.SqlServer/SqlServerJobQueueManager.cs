using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
namespace OddJob.SqlServer
{
    public class SqlServerJobQueueManager : IJobQueueManager
    {
        public int FetchSize { get; protected set; }
        public string QueueTableName { get { return "MainQueueTable"; } }
        public string ParamTableName { get { return "QueueParamValue"; } }
        private SqlConnection conn { get; set; }
        public IEnumerable<IOddJob> GetJobs(string[] queueNames)
        {
            var grid = conn.QueryMultiple(GetJobSqlString, new { queueNames = queueNames });
            var baseJobs = grid.Read<SqlServerDbOddJobMetaData>();
            var jobMetaData = grid.Read<SqlServerOddJobParamMetaData>();
            return baseJobs
                .GroupJoin(jobMetaData,
                q => q.JobId,
                r => r.JobId,
                (q, r) =>
                new SqlServerDbOddJob()
                {
                    JobId = q.JobGuid,
                    MethodName = q.MethodName,
                    TypeExecutedOn = Type.GetType(q.TypeExecutedOn),
                    JobArgs = r.OrderBy(p => p.ParamOrdinal)
                    .Select(s =>
                    Newtonsoft.Json.JsonConvert.DeserializeObject(s.SerializedValue,Type.GetType(s.SerializedType,false))).ToArray()
                });

        }
        public string GetJobSqlString {get{
                return
string.Format(@"select top {0}
         JobGuid, QueueName,TypeExecutedOn,
         MethodName,Status, 
         DoNotExecuteBefore 
        from {1} 
        where QueueName in (@queueNames) 
            and (DoNotExecuteBefore <=get_date() 
               or DoNotExecuteBefore is null)
        select JobId, ParamOrdinal,SerializedValue, SerializedType
from {2} where JobId in (select top {0}
         QueueName,TypeExecutedOn,
         MethodName,Status, 
         DoNotExecuteBefore 
        from {1} 
        where QueueName in (@queueNames) 
            and (DoNotExecuteBefore <=get_date() 
               or DoNotExecuteBefore is null))"
, FetchSize, QueueTableName,ParamTableName);
} }

        public void MarkJobFailed(Guid jobGuid)
        {
            throw new NotImplementedException();
        }

        public void MarkJobSuccess(Guid jobGuid)
        {
            throw new NotImplementedException();
        }

        public void MarkJobInProgress(Guid jobId)
        {
            throw new NotImplementedException();
        }
    }
}
