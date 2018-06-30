using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dapper;
using OddJob.Storage.TestKit;
using Xunit.Abstractions;
using Timer = System.Timers.Timer;

namespace OddJob.Storage.SqlServer.Test
{
    public class SqlServerStorageTest: StorageTests
    {
        public SqlServerStorageTest()
        {
            var execPath = Path.Combine(Assembly.GetExecutingAssembly().CodeBase, string.Empty)
                .Substring(0, Assembly.GetExecutingAssembly().CodeBase.LastIndexOf('/'));
            AppDomain.CurrentDomain.SetData("DataDirectory", new Uri(Path.Combine(execPath, string.Empty)).LocalPath);
            UnitTestTableHelper.EnsureTablesExist();

            
        }

        protected override Func<IJobQueueAdder> jobAddStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueAdder(new TestConnectionFactory(),
                    new SqlServerJobQueueDefaultTableConfiguration());
            }
        }

        protected override Func<IJobQueueManager> jobMgrStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueManager(new TestConnectionFactory(),
                    new SqlServerJobQueueDefaultTableConfiguration());
            }
        }

    }
}


