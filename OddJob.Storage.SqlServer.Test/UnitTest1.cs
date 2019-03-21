using System;
using System.IO;
using System.Reflection;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.Sql.Common;
using LinqToDB.DataProvider.SqlServer;
using OddJob.Storage.Sql.SqlServer.Test;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer.Test
{
    public class SqlServerStorageTest : StorageTests
    {
        public SqlServerStorageTest()
        {
            var execPath = Path.Combine(Assembly.GetExecutingAssembly().CodeBase, string.Empty)
                .Substring(0, Assembly.GetExecutingAssembly().CodeBase.LastIndexOf('/'));
            AppDomain.CurrentDomain.SetData("DataDirectory", new Uri(Path.Combine(execPath, string.Empty)).LocalPath);
            UnitTestTableHelper.EnsureTablesExist();


        }

        protected override Func<IJobQueueAdder> JobAddStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueAdder(
                    new SqlServerDataConnectionFactory(new TestDbConnectionFactory(), SqlServerVersion.v2008)
                    ,
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }

        protected override Func<IJobQueueManager> JobMgrStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueManager(
                    new SqlServerDataConnectionFactory(new TestDbConnectionFactory(), SqlServerVersion.v2008),
                    new SqlDbJobQueueDefaultTableConfiguration(), new NullOnMissingTypeJobTypeResolver());
            }
        }

    }
}


