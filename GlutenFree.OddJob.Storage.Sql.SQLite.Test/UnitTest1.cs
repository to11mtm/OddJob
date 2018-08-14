using System;
using System.Data.SQLite;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Storage.Sql.SQLite.Test
{
    public static class UnitTestTableHelper
    {
        internal static readonly string connString = "FullUri=file::memory:?cache=shared";
        /// <summary>
        /// This is here because SQLite will only hold In-memory DBs as long as ONE connection is open. so we just open one here and keep it around for appdomain life.
        /// </summary>
        public static readonly SQLiteConnection heldConnection;

        static UnitTestTableHelper()
        {
            heldConnection = new SQLiteConnection(connString);
        }
        public static void EnsureTablesExist()
        {
            
            heldConnection.Open();
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
                    cmd.CommandText = SQLiteDbJobTableHelper.JobQueueParamTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SQLiteDbJobTableHelper.JobTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }




        }
    }

    public class SQLiteStorageTest : StorageTests
    {
        public SQLiteStorageTest()
        {

            UnitTestTableHelper.EnsureTablesExist();


        }

        protected override Func<IJobQueueAdder> JobAddStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueAdder(
                    new SQLiteJobQueueDbConnectionFactory(UnitTestTableHelper.connString)
                    ,
                    new SqlDbJobQueueDefaultTableConfiguration());
            }
        }

        protected override Func<IJobQueueManager> JobMgrStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueManager(
                    new SQLiteJobQueueDbConnectionFactory(UnitTestTableHelper.connString),
                    new SqlDbJobQueueDefaultTableConfiguration());
            }
        }

    }
}
