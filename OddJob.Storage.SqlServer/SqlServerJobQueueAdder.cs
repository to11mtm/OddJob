using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder 
    {
      

        public SqlServerJobQueueAdder(IJobQueueDbConnectionFactory jobQueueDbConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDbConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
