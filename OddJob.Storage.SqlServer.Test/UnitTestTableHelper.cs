using GlutenFree.OddJob.Storage.Sql.SqlServer;

namespace OddJob.Storage.Sql.SqlServer.Test
{
    public static class UnitTestTableHelper
    {
        public static void EnsureTablesExist()
        {
            using (var db = SqlConnectionHelper.GetLocalDB("unittestdb"))
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
                    cmd.CommandText = SqlServerDbJobTableHelper.JobQueueParamTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SqlServerDbJobTableHelper.JobTableCreateScript(
                        new SqlDbJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }

  
                
                
        }
    }
}