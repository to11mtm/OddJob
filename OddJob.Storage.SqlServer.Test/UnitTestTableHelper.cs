namespace OddJob.Storage.SqlServer.Test
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
  DROP TABLE dbo.{0}; ", SqlServerJobQueueDefaultTableConfiguration.DefaultQueueTableName);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = string.Format(@"IF OBJECT_ID('dbo.{0}', 'U') IS NOT NULL 
  DROP TABLE dbo.{0}; ", SqlServerJobQueueDefaultTableConfiguration.DefaultQueueParamTableName);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SqlServerDbJobTableHelper.JobQueueParamTableCreateScript(
                        new SqlServerJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = SqlServerDbJobTableHelper.JobTableCreateScript(
                        new SqlServerJobQueueDefaultTableConfiguration());
                    cmd.ExecuteNonQuery();
                }
            }

  
                
                
        }
    }
}