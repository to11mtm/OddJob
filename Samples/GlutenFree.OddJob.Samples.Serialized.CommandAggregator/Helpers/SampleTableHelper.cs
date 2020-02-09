using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    public static class SampleTableHelper
    {
        internal static readonly string connString = "FullURI=file::memory:?cache=shared"; //"FullUri=file::memory:?cache=shared";
        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        public static bool TablesCreated = false;
        static SampleTableHelper()
        {
            heldConnection = new SQLiteConnection(connString);
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
                using (var db = new SQLiteConnection(connString))
                {
                    db.Open();
                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", tableConfiguration.QueueTableName);
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = db.CreateCommand())
                    {
                        cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", tableConfiguration.ParamTableName);
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
    }
}