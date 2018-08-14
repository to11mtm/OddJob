using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB.DataProvider.SQLite;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder<SqlServerDbConnectionFactory>
    {
      

        public SqlServerJobQueueAdder(SqlServerDbConnectionFactory jobQueueDbConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDbConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
