using GlutenFree.OddJob.Storage.Sql.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.Postgres
{
    public class PostgresJobQueueDataConnectionFactory : IJobQueueDataConnectionFactory
    {
        private readonly string _connectionString;

        public PostgresJobQueueDataConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataConnection CreateDataConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(ProviderName.PostgreSQL, _connectionString, mappingSchema);
        }
    }
}

