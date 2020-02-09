using GlutenFree.Linq2Db.Helpers;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebSharper;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
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

    //TODO: Figure out how to get Handler Interface pattern working with Websharper.
    public interface IRemotingHandler<TRemotingCommand, TRemotingTaskResultType>
    {
        [Remote]
        Task<TRemotingTaskResultType> Handle(TRemotingCommand command);
    }

    public class OddJobRemotingHelper
    {
        
    }

    [JavaScript]
    public class QueueNameListRequest
    {

    }

    [JavaScript]
    public class GetMethodsForQueueNameRequest
    {
        public string QueueName;
    }


    public class OddJobRemotingHandler : IRemotingHandler<JobUpdateViewModel, bool>, IRemotingHandler<QueueNameListRequest,string[]>, 
        IRemotingHandler<JobSearchCriteria,JobMetadataResult[]>, IRemotingHandler<GetMethodsForQueueNameRequest,string[]>
    {
        private IJobSearchProvider _provider;
        public OddJobRemotingHandler(IJobSearchProvider provider)
        {
            _provider = provider;
        }

        public Task<string[]> Handle(QueueNameListRequest req)
        {
            var result = (new string[] { null }).Concat(_provider.GetJobCriteriaValues(q => q.QueueName).ToList()).ToArray();
            return Task.FromResult(result);
        }

        public Task<JobMetadataResult[]> Handle(JobSearchCriteria criteria)
        {
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
            var result = expr == null ? new JobMetadataResult[] { } : _provider.GetSerializableJobsByCriteria(expr).Select(q => new JobMetadataResult()
            {
                ExecutionTime = q.ExecutionTime.ToString(),
                JobArgs = q.JobArgs.Select(r => new JobParameterDto()
                { Ordinal = r.Ordinal, Name = r.Name, Type = r.TypeName, Value = r.Value }).ToArray(),
                JobId = q.JobId,
                MethodGenericTypes = q.MethodGenericTypes.ToArray(),
                MethodName = q.MethodName,
                Queue = q.QueueName,
                RetryParameters = new JobRetryParameters()
                {
                    LastAttempt = q.RetryParameters.LastAttempt,
                    RetryCount = q.RetryParameters.RetryCount,
                    MaxRetries = q.RetryParameters.MaxRetries,
                    MinRetryWait = q.RetryParameters.MinRetryWait
                },

                Status = q.Status,
                TypeExecutedOn = q.TypeExecutedOn
            });
            return Task.FromResult(result.ToArray());
        }

        public Task<string[]> Handle(GetMethodsForQueueNameRequest queueNameRequest)
        {

            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            var results = new string[] { null };
            if (!string.IsNullOrWhiteSpace(queueNameRequest.QueueName))
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.QueueName.ToLower().Contains(queueNameRequest.QueueName.ToLower()), false);
                results = results.Concat(_provider.GetJobCriteriaByCriteria(expr, q => q.MethodName)).ToArray();
            }


            return Task.FromResult(results);
        }

        
        public Task<bool> Handle(JobUpdateViewModel input)
        {
            Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> updateSet =
                new Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object>();
            Dictionary<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>> updateParamSet =
                new Dictionary<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>();

            if (input.UpdateData.UpdateMethodName && !string.IsNullOrWhiteSpace(input.UpdateData.NewQueueName))
            {
                updateSet.Add((a => a.MethodName), input.UpdateData.NewMethodName);
            }

            if (input.UpdateData.UpdateQueueName && !string.IsNullOrWhiteSpace(input.UpdateData.NewQueueName))
            {
                updateSet.Add(a => a.QueueName, input.UpdateData.NewQueueName);
            }
            if (input.UpdateData.UpdateRetryCount)
            {
                updateSet.Add(a => a.MaxRetries, input.UpdateData.NewMaxRetryCount);
            }

            if (input.UpdateData.UpdateStatus && !string.IsNullOrWhiteSpace(input.UpdateData.NewStatus))
            {
                updateSet.Add(a => a.Status, input.UpdateData.NewStatus);
            }
            string statusIfRequired = input.UpdateData.RequireOldStatus ? input.UpdateData.OldStatus : "";

            updateParamSet = input.UpdateData.ParamUpdates.Select(q => BuildParamUpdateSet(q)).ToDictionary(q => q.Key, q => q.Value);

            var result = _provider.UpdateJobMetadataAndParameters(new JobUpdateCommand()
            {
                JobGuid = input.UpdateData.JobGuid,
                OldStatusIfRequired = statusIfRequired,
                SetJobParameters = updateParamSet,
                SetJobMetadata = updateSet
            });



            return Task.FromResult(result);
        }

        private static KeyValuePair<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>
            BuildParamUpdateSet(UpdateForParam updateForParam)
        {
            Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object> updateDict =
                new Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>();
            if (updateForParam.UpdateParamType == true)
            {
                updateDict.Add(q => q.SerializedType, updateForParam.NewParamType);
            }

            if (updateForParam.UpdateParamValue == true)
            {
                updateDict.Add(q => q.SerializedValue, updateForParam.NewParamValue);
            }

            return new KeyValuePair<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>(
                updateForParam.ParamOrdinal, updateDict);


        }

        
    }
    /*
    public class OddJobRemoting
    {
        private IJobSearchProvider _provider;
        public OddJobRemoting(IJobSearchProvider provider)
        {
            _provider = provider;
        }

        [Remote]
        public Task<string[]> Handle()
        {
            var result = (new string[]{null}).Concat(_provider.GetJobCriteriaValues(q => q.QueueName).ToList()).ToArray();
            return Task.FromResult(result);
        }

        [Remote]
        public Task<JobMetadataResult[]> Handle(JobSearchCriteria criteria )
        {
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

            var result = expr == null ? new JobMetadataResult[]{} : _provider.GetSerializableJobsByCriteria(expr).Select(q => new JobMetadataResult()
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
        public Task<string> DoSomething(JobSearchCriteria input)
        {
            return Task.FromResult(new String(input.ToString().ToCharArray().Reverse().ToArray()));
        }
        [Remote]
        public Task<string[]> Handle(string queueName)
        {
            
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            var results = new string[]{null};
            if (!string.IsNullOrWhiteSpace(queueName))
            {
                expr = ExpressionHelpers.CombineBinaryExpression(expr,
                    (a) => a.QueueName.ToLower().Contains(queueName.ToLower()), false);
                results = results.Concat(_provider.GetJobCriteriaByCriteria(expr, q => q.MethodName)).ToArray();
            }

            
            return Task.FromResult(results);
        }

        [Remote]
        public Task<bool> Handle(JobUpdateViewModel input)
        {
            Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> updateSet =
                new Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object>();
            Dictionary<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>> updateParamSet =
                new Dictionary<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>();
                
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

            updateParamSet = input.UpdateData.ParamUpdates.Select(q => BuildParamUpdateSet(q)).ToDictionary(q=>q.Key,q=>q.Value);

            var result = _provider.UpdateJobMetadataAndParameters(new JobUpdateCommand()
            {
                JobGuid = input.UpdateData.JobGuid, OldStatusIfRequired = statusIfRequired,
                SetJobParameters = updateParamSet, SetJobMetadata = updateSet
            });
            
            
            
            return Task.FromResult(result);
        }

        private static KeyValuePair<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>
            BuildParamUpdateSet(UpdateForParam updateForParam)
        {
            Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object> updateDict =
                new Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>();
            if (updateForParam.UpdateParamType == true)
            {
                updateDict.Add(q => q.SerializedType, updateForParam.NewParamType);
            }

            if (updateForParam.UpdateParamValue == true)
            {
                updateDict.Add(q => q.SerializedValue, updateForParam.NewParamValue);
            }

            return new KeyValuePair<int, Dictionary<Expression<Func<SqlCommonOddJobParamMetaData, object>>, object>>(
                updateForParam.ParamOrdinal, updateDict);


        }
    }*/
}