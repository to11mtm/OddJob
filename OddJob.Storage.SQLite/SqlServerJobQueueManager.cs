using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
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

    public class SQLiteOddjobQueueManager : BaseSqlJobQueueManager<SQLiteJobQueueDbConnectionFactory>
    {
        
        public SQLiteOddjobQueueManager(SQLiteJobQueueDbConnectionFactory jobQueueConnectionFactory,
            ISqlDbJobQueueTableConfiguration tableConfig) : base(jobQueueConnectionFactory,
            tableConfig)
        {

        }
        
    }
}
