using System.Runtime.CompilerServices;
using FluentMigrator.Runner.Generators.SqlServer;
using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.TableHelper;
using LinqToDB.DataProvider.SqlServer;

namespace OddJob.Storage.Sql.SqlServer.Test
{
    public static class UnitTestTableHelper
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void EnsureTablesExist()
        {
            using (var db = SqlConnectionHelper.GetLocalDB("unittestdb"))
            {
                var helper =
                    new SqlTableHelper(
                        new SqlServerDataConnectionFactory(new TestDbConnectionFactory(), SqlServerVersion.v2008),
                        new SqlServer2008Generator());

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
                    cmd.CommandText = string.Format(@"IF OBJECT_ID('dbo.{0}', 'U') IS NOT NULL 
  DROP TABLE dbo.{0}; ", SqlDbJobQueueDefaultTableConfiguration.DefaultJobMethodGenericParamTableName);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetMainTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        //SqlServerDbJobTableHelper.JobQueueParamTableCreateScript(
                        //new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetParamTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        //SqlServerDbJobTableHelper.JobTableCreateScript(
                        //new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText =
                        helper.GetGenericParamTableSql(
                            new SqlDbJobQueueDefaultTableConfiguration());
                        //SqlServerDbJobTableHelper.JobQueueJobMethodGenericParamTableCreateScript(
                        //new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }

  
                
                
        }
    }
}