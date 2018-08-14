using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerDBConnectionFactory : IJobQueueDbConnectionFactory
    {
        public string _connectionString { get; private set; }

        public SqlServerDBConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DataConnection CreateDbConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(ProviderName.SqlServer, _connectionString);
        }
    }
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder<SqlServerDBConnectionFactory>
    {
      

        public SqlServerJobQueueAdder(SqlServerDBConnectionFactory jobQueueDbConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDbConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
