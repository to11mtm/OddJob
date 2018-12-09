using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using GlutenFree.Linq2Db.Helpers;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.SQLite;
using WebSharper;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    [JavaScript]
    public class JobSearchCriteria
    {
        public string QueueName { get; set; }
        public string MethodName { get; set; }
        public Guid? JobGuid { get; set; }
    }

    [JavaScript]
    public class JobRetryParameters
    {
        public int MaxRetries { get; set; }
        public TimeSpan MinRetryWait { get; set; }
        public int RetryCount { get; set; }
        public DateTime? LastAttempt { get; set; }
    }

    [JavaScript]
    public class JobParameterDto
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    [JavaScript]
    public class JobMetadataResult
    {
        public Guid JobId { get; set; }
        public JobParameterDto[] JobArgs { get; set; }
        public string TypeExecutedOn { get; set; }
        public string MethodName { get; set; }
        public string Status { get; set; }
        public string[] MethodGenericTypes { get; set; }
        public string ExecutionTime { get; set; }
        public JobRetryParameters RetryParameters { get; set; }
        public string Queue { get; set; }
    }

    public static class Remoting
    {
        [Remote]
        public static Task<JobMetadataResult[]> SearchCriteria(string queueName, string methodName)
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            expr = ExpressionHelpers.CombineBinaryExpression(expr,
                (a) => a.MethodName.ToLower().Contains(methodName.ToLower()), false);
            /*       if (statusCriteria != null && statusCriteria.Any())
                   {
                       expr = ExpressionHelpers.CombineBinaryExpression(expr, (a) => a.Status.LikeAnyLower(statusCriteria),
                           requireAll);
       
                   }
       
                   if (jobNameCriteria != null && jobNameCriteria.Any())
                   {
                       expr = ExpressionHelpers.CombineBinaryExpression(expr,
                           (a) => a.MethodName.LikeAnyUpper(jobNameCriteria), requireAll);
                   }
       
                   if (jobGuids != null && jobGuids.Any())
                   {
                       expr = ExpressionHelpers.CombineBinaryExpression(expr, (a) => a.JobGuid.In(jobGuids), requireAll);
                   }
       
                   expr = ExpressionHelpers.CombineBinaryExpression(expr, (a) => (
                       (createdNoLaterThan == null || a.CreatedDate <= createdNoLaterThan)
                       &&
                       (createdNoEarlierThan == null || a.CreatedDate >= createdNoEarlierThan)
                   ), requireAll);
                   expr = ExpressionHelpers.CombineBinaryExpression(expr, (a) => (
                       (lastExecutedNoLaterThan == null || a.LastAttempt <= lastExecutedNoLaterThan)
                       &&
                       (lastExecutedNoEarlierThan == null || a.LastAttempt >= lastExecutedNoEarlierThan)
                   ), requireAll);
                   */
            var result = manager.GetJobsByCriteria(expr).Select(q => new JobMetadataResult()
            {
                ExecutionTime = q.ExecutionTime.ToString(),
                JobArgs = q.JobArgs.Select(r => new JobParameterDto()
                    {Name = r.Name, Type = r.Type, Value = r.Value.ToString()}).ToArray(),
                JobId = q.JobId,
                MethodGenericTypes = q.MethodGenericTypes.Select(r => r.AssemblyQualifiedName).ToArray(),
                MethodName = q.MethodName, Queue = q.Queue,
                RetryParameters = new JobRetryParameters()
                {
                    LastAttempt = q.RetryParameters.LastAttempt, RetryCount = q.RetryParameters.RetryCount,
                    MaxRetries = q.RetryParameters.MaxRetries, MinRetryWait = q.RetryParameters.MinRetryWait
                },

                Status = q.Status, TypeExecutedOn = q.TypeExecutedOn.AssemblyQualifiedName
            });
            return Task.FromResult(result.ToArray());
        }

        [Remote]
        public static Task<string> DoSomething(JobSearchCriteria input)
        {
            return Task.FromResult(new String(input.ToString().ToCharArray().Reverse().ToArray()));
        }
    }
}