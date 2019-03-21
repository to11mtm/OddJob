using System.Collections.Generic;
using System.Linq.Expressions;
using GlutenFree.Linq2Db.Helpers;
using GlutenFree.OddJob.Storage.Sql.Common;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SQLite
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
            return new ExpressionReplacingLinq2DbConnection(ProviderName.SQLiteClassic, _connectionString, mappingSchema,new List<ExpressionVisitor>());
        }
    }
}