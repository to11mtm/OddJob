using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueDataConnectionFactory : IJobQueueDataConnectionFactory
    {
        private readonly string _connectionString;

        public SQLiteJobQueueDataConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataConnection CreateDataConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(ProviderName.SQLite, _connectionString, mappingSchema);
        }
    }
}