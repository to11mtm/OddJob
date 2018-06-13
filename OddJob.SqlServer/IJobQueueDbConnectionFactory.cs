using System.Data.SqlClient;
namespace OddJob.SqlServer
{
    public interface IJobQueueDbConnectionFactory
    {
        SqlConnection GetConnection();
    }
}
