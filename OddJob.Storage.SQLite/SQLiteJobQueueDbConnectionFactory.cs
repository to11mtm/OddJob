using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.SQLite
{
    public class SQLiteJobQueueDbConnectionFactory : IJobQueueDbConnectionFactory
    {
        private string _connectionString;

        public SQLiteJobQueueDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataConnection CreateDbConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(ProviderName.SQLite, _connectionString, mappingSchema);
        }
    }
}