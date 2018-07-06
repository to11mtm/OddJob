using System.Data.SqlClient;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface IJobQueueDbConnectionFactory
    {
        SqlConnection GetConnection();
    }
}
