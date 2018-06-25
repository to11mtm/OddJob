using System.Data.SqlClient;

namespace OddJob.Storage.SqlServer
{
    public interface IJobQueueDbConnectionFactory
    {
        SqlConnection GetConnection();
    }
}
