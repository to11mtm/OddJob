using System;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.Sql.SqlServer;

namespace GlutenFree.OddJob.Storage.Sql.SQLite.Tests
{
    public static class UnitTestTableHelper
    {
        public static void EnsureTablesExist()
        {
            using (var db = .GetLocalDB("unittestdb"))
            {

                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"IF OBJECT_ID('dbo.{0}', 'U') IS NOT NULL 
  DROP TABLE dbo.{0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueTableName);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"IF OBJECT_ID('dbo.{0}', 'U') IS NOT NULL 
  DROP TABLE dbo.{0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultQueueParamTableName);
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
    public class SqlServerStorageTest : StorageTests
    {
        public SqlServerStorageTest()
        {
            
            UnitTestTableHelper.EnsureTablesExist();


        }

        protected override Func<IJobQueueAdder> jobAddStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueAdder(
                    new SQLiteJobQueueDbConnectionFactory(SqlConnectionHelper.CheckConnString("unittestdb"))
                    ,
                    new SqlDbJobQueueDefaultTableConfiguration());
            }
        }

        protected override Func<IJobQueueManager> jobMgrStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueAdder(
                    new SQLiteJobQueueDbConnectionFactory(SqlConnectionHelper.CheckConnString("unittestdb")),
                    new SqlDbJobQueueDefaultTableConfiguration());
            }
        }

    }
}
