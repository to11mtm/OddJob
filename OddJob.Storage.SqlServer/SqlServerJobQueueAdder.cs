﻿using GlutenFree.OddJob.Storage.SQL.Common;
using GlutenFree.OddJob.Storage.SQL.Common.DbDtos;
using LinqToDB.DataProvider.SQLite;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer
{
    public class SqlServerJobQueueAdder : BaseSqlJobQueueAdder
    {
      

        public SqlServerJobQueueAdder(SqlServerDataConnectionFactory jobQueueDataConnectionFactory, SQL.Common.ISqlDbJobQueueTableConfiguration jobQueueTableConfiguration) : base(jobQueueDataConnectionFactory, jobQueueTableConfiguration)
        {
        }
    }
}
