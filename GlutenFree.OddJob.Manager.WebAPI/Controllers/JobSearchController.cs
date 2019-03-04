using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GlutenFree.Linq2Db.Helpers;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using GlutenFree.OddJob.Storage.SQL.SQLite;
using LinqToDB.Expressions;
using LinqToDB.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GlutenFree.OddJob.Manager.WebAPI.Controllers
{
    public interface IJob1
    {
        void Job1Method1();
        void Job1Method2();
    }

    public interface IJob2
    {
        void Job2Method1();
        void Job2Method2(string param1);
    }

    public static class TempDevInfo
    {
        internal static readonly string
            ConnString = "FullURI=file::memory:?cache=shared"; //"FullUri=file::memory:?cache=shared";

        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        public static bool TablesCreated = false;

        static TempDevInfo()
        {
            heldConnection = new SQLiteConnection(ConnString);
            EnsureTablesExist(TableConfigurations.Values.Append(new SqlDbJobQueueDefaultTableConfiguration()));
            var sampleDataAdder1 = new SQLiteJobQueueAdder(new SQLiteJobQueueDataConnectionFactory(ConnString),
                new QueueNameBasedJobAdderQueueTableResolver(TableConfigurations,
                    new SqlDbJobQueueDefaultTableConfiguration()));
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method1(), new RetryParameters(), null, "console");
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method2(), new RetryParameters(), null, "console");
            sampleDataAdder1.AddJob((IJob1 j) => j.Job1Method1(), new RetryParameters(), null, "counter");
            sampleDataAdder1.AddJob((IJob2 j) => j.Job2Method1(), new RetryParameters(), null, "counter");
            sampleDataAdder1.AddJob((IJob2 j) => j.Job2Method2("derp"), new RetryParameters(), queueName: "console");
        }

        //We only want this to execute once for the sample.
        //You would want to get rid of the DROPs in a real world scenario, and instead check that you aren't breaking anything.
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void EnsureTablesExist(IEnumerable<ISqlDbJobQueueTableConfiguration> configs)
        {
            if (TablesCreated)
            {
                return;
                ;
            }

            if (heldConnection.State != ConnectionState.Open)
            {
                heldConnection.Open();
            }

            foreach (var tableConfiguration in configs)
            {
                using (var db = new SQLiteConnection(ConnString))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.QueueTableName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.ParamTableName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                            tableConfiguration.JobMethodGenericParamTableName);
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = SQLiteDbJobTableHelper.JobQueueParamTableCreateScript(
                            tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = SQLiteDbJobTableHelper.JobTableCreateScript(
                            tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            SQLiteDbJobTableHelper.JobQueueJobMethodGenericParamTableCreateScript(
                                tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText =
                            SQLiteDbJobTableHelper.SuggestedIndexes(tableConfiguration);
                        cmd.ExecuteNonQuery();
                    }

                }
            }


            TablesCreated = true;




        }

        public static Dictionary<string, ISqlDbJobQueueTableConfiguration> TableConfigurations
        {
            get
            {
                return new Dictionary<string, ISqlDbJobQueueTableConfiguration>()
                {
                    {
                        "console",
                        new MyTableConfigs()
                        {
                            QueueTableName = "consoleQueue", ParamTableName = "consoleParam",
                            JobMethodGenericParamTableName = "consoleGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    },
                    {
                        "counter",
                        new MyTableConfigs()
                        {
                            QueueTableName = "counterQueue", ParamTableName = "counterParam",
                            JobMethodGenericParamTableName = "counterGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    }
                };
            }
        }


    }


    public class MyTableConfigs : ISqlDbJobQueueTableConfiguration
    {
        public string QueueTableName { get; set; }
        public string ParamTableName { get; set; }
        public int JobClaimLockTimeoutInSeconds { get; set; }
        public string JobMethodGenericParamTableName { get; set; }
    }

    [Route("api/[controller]")]
    //[ApiController]
    public class SharedJobQueueController : ControllerBase
    {
        [HttpGet("getjobs")]
        public IEnumerable<IOddJobWithMetadata> GetJobs()
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            return manager.GetJobsByCriteria((a) => true);
        }




        [HttpGet("jobsByCriteria")]
        public IEnumerable<IOddJobWithMetadata> JobsByCriteria(bool requireAll, IEnumerable<string> statusCriteria,
            IEnumerable<string> jobNameCriteria, DateTime? createdNoLaterThan,
            DateTime? createdNoEarlierThan, DateTime? lastExecutedNoLaterThan, DateTime? lastExecutedNoEarlierThan,
            IEnumerable<Guid> jobGuids)
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Expression<Func<SqlCommonDbOddJobMetaData, bool>> expr = null;
            if (statusCriteria != null && statusCriteria.Any())
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


            return manager.GetJobsByCriteria(expr);
        }

        [HttpPost("setJobValues")]
        public bool SetJobValues(Guid jobGuid, string status, string jobMethod, int? maxRetryCount,
            DateTimeOffset? doNotExecuteBefore, int? minRetryWait, bool clearLockTime, bool clearLockGuid,
            string typeExecutedOn, string requiredOldStatus)
        {
            var manager = new SQLiteJobQueueManager(new SQLiteJobQueueDataConnectionFactory(TempDevInfo.ConnString),
                TempDevInfo.TableConfigurations["console"], new NullOnMissingTypeJobTypeResolver());
            Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object> exprDictionary =
                new Dictionary<Expression<Func<SqlCommonDbOddJobMetaData, object>>, object>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                exprDictionary.Add(q => q.Status, status);
            }

            if (!string.IsNullOrWhiteSpace(jobMethod))
            {
                exprDictionary.Add(q => q.MethodName, jobMethod);
            }

            if (maxRetryCount != null)
            {
                exprDictionary.Add(q => q.MaxRetries, maxRetryCount);
            }

            if (doNotExecuteBefore != null)
            {
                exprDictionary.Add(q => q.DoNotExecuteBefore, doNotExecuteBefore);
            }

            if (minRetryWait != null)
            {
                exprDictionary.Add(q => q.MinRetryWait, minRetryWait);
            }

            if (typeExecutedOn != null)
            {
                exprDictionary.Add(q => q.TypeExecutedOn, typeExecutedOn);
            }

            if (clearLockGuid)
            {
                exprDictionary.Add(q => q.LockGuid, null);
            }

            if (clearLockTime)
            {
                exprDictionary.Add(q => q.LockClaimTime, null);
            }

            return manager.UpdateJobMetadataValues(exprDictionary, jobGuid, requiredOldStatus);
        }
    }
}