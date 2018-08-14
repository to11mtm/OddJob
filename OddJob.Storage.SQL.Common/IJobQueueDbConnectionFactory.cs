using System.Data.Common;
using System.Data.SqlClient;
using LinqToDB.Data;
using LinqToDB.DataProvider.SapHana;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface IJobQueueDbConnectionFactory
    {
        DataConnection CreateDbConnection(MappingSchema mappingSchema);
    }

    public class JobQueueDbConnectionFactorySettings
    {
        public string ConnectionString { get; set; }
        public string ProviderName { get; set; }
    }
}
