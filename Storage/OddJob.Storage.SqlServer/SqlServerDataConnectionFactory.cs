using GlutenFree.OddJob.Storage.Sql.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SqlServer;
using LinqToDB.Mapping;
using SqlServerTools = LinqToDB.DataProvider.SqlCe.SqlServerTools;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerDataConnectionFactory : IJobQueueDataConnectionFactory
    {
        
        private readonly IJobQueueDbConnectionFactory _connectionFactory;
        private readonly SqlServerVersion _sqlServerVersion;
        //private readonly DataOptions _dataOpts;

        public SqlServerDataConnectionFactory(IJobQueueDbConnectionFactory connectionFactory, SqlServerVersion sqlServerVersion)
        {
            _connectionFactory = connectionFactory;
            //_dataOpts =  new DataOptions(new ConnectionOptions("sqlserver-oddjob", ConnectionFactory: dopt=> _connectionFactory.CreateDbConnection(), ProviderName: ProviderName.SqlServer));
            _sqlServerVersion = sqlServerVersion;
        }
        public DataConnection CreateDataConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(new DataOptions(new ConnectionOptions("sqlserver-oddjob",
                ConnectionFactory: dopt => _connectionFactory.CreateDbConnection(),
                ProviderName: ProviderName.SqlServer, MappingSchema: mappingSchema)));
        }
    }
}