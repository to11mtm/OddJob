using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerDataConnectionFactory : IJobQueueDataConnectionFactory
    {
        
        private readonly IJobQueueDbConnectionFactory _connectionFactory;
        private readonly SqlServerVersion _sqlServerVersion;
        public SqlServerDataConnectionFactory(IJobQueueDbConnectionFactory connectionFactory, SqlServerVersion sqlServerVersion)
        {
            _connectionFactory = connectionFactory;
            _sqlServerVersion = sqlServerVersion;
        }
        public DataConnection CreateDataConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(new SqlServerDataProvider("sqlserver-oddjob",_sqlServerVersion), _connectionFactory.CreateDbConnection(),true).AddMappingSchema(mappingSchema);
            
        }
    }
}