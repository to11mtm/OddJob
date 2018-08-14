using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerDbConnectionFactory : IJobQueueDbConnectionFactory
    {
        public string _connectionString { get; private set; }

        public SqlServerDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataConnection CreateDbConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(ProviderName.SqlServer, _connectionString);
        }
    }
}