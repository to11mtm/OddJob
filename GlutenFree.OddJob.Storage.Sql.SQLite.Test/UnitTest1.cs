using System;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.Sql.Common;

namespace GlutenFree.OddJob.Storage.Sql.SQLite.Test
{
    public class SQLiteStorageTest : StorageTests
    {
        public SQLiteStorageTest()
        {

            UnitTestTableHelper.EnsureTablesExist();


        }

        protected override Func<IJobQueueAdder> JobAddStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueAdder(
                    new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString)
                    ,
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }

        protected override Func<IJobQueueManager> JobMgrStoreFunc
        {
            get
            {
                return () => new SQLiteJobQueueManager(
                    new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper.connString),
                    new SqlDbJobQueueDefaultTableConfiguration(),new NullOnMissingTypeJobTypeResolver());
            }
        }

    }
}
