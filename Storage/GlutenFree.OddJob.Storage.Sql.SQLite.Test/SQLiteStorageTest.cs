using System;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.Sql.BaseTests;
using GlutenFree.OddJob.Storage.Sql.Common;
using Xunit.Abstractions;

namespace GlutenFree.OddJob.Storage.Sql.SQLite.Test
{
    public class SQLiteStorageTest : SqlStorageTests
    {
        public SQLiteStorageTest(ITestOutputHelper outputHelper) :base(outputHelper)
        {

            SQLiteUnitTestTableHelper.EnsureTablesExist();


        }

        protected override Func<IJobQueueAdder> JobAddStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueAdder(
                    new SQLiteJobQueueDataConnectionFactory(SQLiteUnitTestTableHelper.connString)
                    ,
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }

        protected override Func<IJobQueueManager> JobMgrStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueManager(
                    new SQLiteJobQueueDataConnectionFactory(SQLiteUnitTestTableHelper.connString),
                    new SqlDbJobQueueDefaultTableConfiguration(),new NullOnMissingTypeJobTypeResolver());
            }
        }

    }
}
