using System.Data.SqlClient;
using OddJob.Storage.SQL.Common;

namespace OddJob.Storage.SqlServer.Test
{
    public class TestConnectionFactory : IJobQueueDbConnectionFactory
    {
        public SqlConnection GetConnection()
        {
            return SqlConnectionHelper.GetLocalDB("unittestdb");
        }
    }
}