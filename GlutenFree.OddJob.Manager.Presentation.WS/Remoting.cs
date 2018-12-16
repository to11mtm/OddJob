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
using Microsoft.FSharp.Core;
using WebSharper;

namespace GlutenFree.OddJob.Manager.Presentation.WS
{
    
    [JavaScript]
    public class JobSearchCriteria
    {
        public bool UseMethod;
        public bool UseStatus;
        public bool useCreatedDate;
        public bool useLastAttemptDate;
        public string createdBefore = "";
        public string createdAfter = "";
        public string attemptedBeforeDate = "";
        public string attemptedAfterDate = "";
        public string attemptedBeforeTime = "";
        public string attemptedAfterTime = "";
        public string createdBeforeTime="";
        public string createdAfterTime = "";
        public bool UseQueue = true;

        public string QueueName { get; set; }
        public string MethodName { get; set; }
        public string Status { get; set; }
        public Guid? JobGuid { get; set; }
    }

    [JavaScript]
    public class UpdateForJob
    {
        public Guid JobGuid;
        public string OldStatus;
        public bool UpdateRetryCount;
        public int NewMaxRetryCount;
        
        public bool RequireOldStatus;
        public bool UpdateMethodName;
        public string NewMethodName;
        public bool UpdateQueueName;
        public string NewQueueName;
        public bool UpdateStatus;
        public string NewStatus;
        public UpdateForParam[] ParamUpdates = new UpdateForParam[]{};
    }

    [JavaScript]
    public class UpdateForParam
    {
        public int ParamOrdinal;
        public bool UpdateParamType;
        public string NewParamType;
        public bool UpdateParamValue;
        public string NewParamValue;
    }

    [JavaScript]
    public class JobRetryParameters
    {
        public int MaxRetries;
        public TimeSpan MinRetryWait;
        public int RetryCount;
        public DateTime? LastAttempt;
    }

    [JavaScript]
    public class JobParameterDto
    {
        public int Ordinal;
        public string Type;
        public string Name;
        public string Value;
    }

    [JavaScript]
    public class JobUpdateViewModel
    {
        public Guid JobGuid;
        public UpdateForJob UpdateData;
        public JobMetadataResult MetaData;
    }
    [JavaScript]
    public class JobMetadataResult
    {
        public Guid JobId;
        public JobParameterDto[] JobArgs;
        public string TypeExecutedOn;
        public string MethodName;
        public string Status;
        public string[] MethodGenericTypes;
        public string ExecutionTime;
        public JobRetryParameters RetryParameters;
        public string Queue;
    }

    public static class Remoting
    {

        [Remote]
        public static Task<string[]> GetQueueNameList()
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            var result = (new string[]{null}).Concat(manager.GetJobCriteriaValues(q => q.QueueName).ToList()).ToArray();
            return Task.FromResult(result);
        }

        [Remote]
        public static Task<JobMetadataResult[]> SearchCriteria(JobSearchCriteria criteria )
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            if (!string.IsNullOrWhiteSpace(criteria.QueueName))
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.QueueName.ToLower().Contains(criteria.QueueName.ToLower()), false);
            }

            if (!string.IsNullOrWhiteSpace(criteria.MethodName) && criteria.UseMethod)
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.MethodName.ToLower().Contains(criteria.MethodName.ToLower()), false);
            }
            if (!string.IsNullOrWhiteSpace(criteria.Status) && criteria.UseStatus)
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.Status.ToLower().Contains(criteria.Status.ToLower()), false);
            }

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
            var result = expr == null ? new JobMetadataResult[]{} : manager.GetSerializableJobsByCriteria(expr).Select(q => new JobMetadataResult()
            {
                ExecutionTime = q.ExecutionTime.ToString(),
                JobArgs = q.JobArgs.Select(r => new JobParameterDto()
                    {Ordinal=r.Ordinal, Name = r.Name, Type = r.TypeName, Value = r.Value}).ToArray(),
                JobId = q.JobId,
                MethodGenericTypes = q.MethodGenericTypes.ToArray(),
                MethodName = q.MethodName, Queue = q.QueueName,
                RetryParameters = new JobRetryParameters()
                {
                    LastAttempt = q.RetryParameters.LastAttempt, RetryCount = q.RetryParameters.RetryCount,
                    MaxRetries = q.RetryParameters.MaxRetries, MinRetryWait = q.RetryParameters.MinRetryWait
                },

                Status = q.Status, TypeExecutedOn = q.TypeExecutedOn
            });
            return Task.FromResult(result.ToArray());
        }

        [Remote]
        public static Task<string> DoSomething(JobSearchCriteria input)
        {
            return Task.FromResult(new String(input.ToString().ToCharArray().Reverse().ToArray()));
        }
        [Remote]
        public static Task<string[]> GetMethods(string queueName)
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            var results = new string[]{null};
            if (!string.IsNullOrWhiteSpace(queueName))
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.QueueName.ToLower().Contains(queueName.ToLower()), false);
                results = results.Concat(manager.GetJobCriteriaByCriteria(expr, q => q.MethodName)).ToArray();
            }

            
            return Task.FromResult(results);
        }

        [Remote]
        public static Task<bool> UpdateJob(JobUpdateViewModel input)
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> updateSet =
                new Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object>();
            if (input.UpdateData.UpdateMethodName && !string.IsNullOrWhiteSpace(input.UpdateData.NewQueueName))
            {
                updateSet.Add((a=>a.MethodName), input.UpdateData.NewMethodName);
            }

            if (input.UpdateData.UpdateQueueName && !string.IsNullOrWhiteSpace(input.UpdateData.NewQueueName))
            {
                updateSet.Add(a=>a.QueueName, input.UpdateData.NewQueueName);
            }
            if(input.UpdateData.UpdateRetryCount)
            {
                updateSet.Add(a => a.MaxRetries, input.UpdateData.NewMaxRetryCount);
            }

            if (input.UpdateData.UpdateStatus && !string.IsNullOrWhiteSpace(input.UpdateData.NewStatus))
            {
                updateSet.Add(a=>a.Status, input.UpdateData.NewStatus);
            }
            string statusIfRequired = input.UpdateData.RequireOldStatus ? input.UpdateData.OldStatus : "";

            
            return Task.FromResult(manager.UpdateJobMetadataValues(updateSet, input.UpdateData.JobGuid, statusIfRequired));
        }
    }
}