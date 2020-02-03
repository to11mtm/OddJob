using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using FluentMigrator.Runner.Generators.SQLite;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite.Test
{
    public static class UnitTestTableHelper
    {
        public static readonly string connString = "FullUri=file::memory:?cache=shared";
        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        public static bool TablesCreated = false;
        static UnitTestTableHelper()
        {
            heldConnection = new SQLiteConnection(connString);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void EnsureTablesExist()
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

            var helper =
                new GlutenFree.OddJob.Storage.Sql.TableHelper.SqlTableHelper(
                    new SQLiteJobQueueDataConnectionFactory(connString),
                    new SQLiteGenerator());
            
            using (var db = new SQLiteConnection(connString))
            {
                db.Open();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueTableName);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueParamTableName);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"DROP TABLE IF EXISTS {0}; ",
                        SqlDbJobQueueDefaultTableConfiguration.DefaultJobMethodGenericParamTableName);
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetMainTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                    //SQLiteDbJobTableHelper.JobQueueParamTableCreateScript(
                    //    new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetParamTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        //SQLiteDbJobTableHelper.JobTableCreateScript(
                        //new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetGenericParamTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        //SQLiteDbJobTableHelper.JobQueueJobMethodGenericParamTableCreateScript(
                        //   new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }

            TablesCreated = true;




        }
    }
}