using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
{
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
                new QueueNameBasedJobAdderQueueTableResolver(new Dictionary<string, ISqlDbJobQueueTableConfiguration>(),
                    TempDevInfo.TableConfigurations["console"]));
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
}