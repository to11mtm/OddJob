using System.Data.SqlClient;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace OddJob.Storage.Sql.SqlServer.Test
{
    public class TestConnectionFactory : IJobQueueDbConnectionFactory
    {
        public SqlConnection GetConnection()
        {
            return SqlConnectionHelper.GetLocalDB("unittestdb");
        }
    }
}