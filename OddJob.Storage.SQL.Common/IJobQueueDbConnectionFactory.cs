using System.Data.SqlClient;

namespace OddJob.Storage.SQL.Common
{
    public interface IJobQueueDbConnectionFactory
    {
        SqlConnection GetConnection();
    }
}
