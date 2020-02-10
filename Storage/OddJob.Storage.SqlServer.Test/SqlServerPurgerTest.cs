﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using LinqToDB.DataProvider.SqlServer;
using OddJob.Storage.Sql.SqlServer.Test;
using Xunit;

namespace GlutenFree.OddJob.Storage.Sql.SqlServer.Test
{
    public class TestConf
    {
        public const string localLocation = "";
        public const string dbName = "unittestdb";

        public static TestDbConnectionFactory FactFact
        {
            get { return new TestDbConnectionFactory(localLocation, dbName); }
        }
    }
    public class SqlServerPurgerTest
    {
        
        public SqlServerPurgerTest()
        {
            var execPath = Path.Combine(Assembly.GetExecutingAssembly().CodeBase, string.Empty)
                .Substring(0, Assembly.GetExecutingAssembly().CodeBase.LastIndexOf('/'));
            AppDomain.CurrentDomain.SetData("DataDirectory", new Uri(Path.Combine(execPath, string.Empty)).LocalPath);
            SqlServerUnitTestTableHelper.EnsureTablesExist(TestConf.dbName, TestConf.localLocation);
        }

        [Fact]
        public void Can_Purge_Jobs_In_State()
        {
            var adder = jobAddStoreFunc();
            var job = adder.AddJob((PurgeTestJob p) => p.JobMethod("canpurgejobsinstate"), queueName: "purge");
            var purger = jobQueuePurgerFunc();
            purger.PurgeQueue("purge",JobStates.New, DateTime.Now);
            var mgr = jobMgrStoreFunc();
            var nullJob = mgr.GetJob(job);
            Xunit.Assert.Null(nullJob);
        }

        protected Func<IJobQueueAdder> jobAddStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueAdder(new SqlServerDataConnectionFactory(TestConf.FactFact, SqlServerVersion.v2008),
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }

        protected Func<IJobQueueManager> jobMgrStoreFunc
        {
            get
            {
                return () => new SqlServerJobQueueManager(new SqlServerDataConnectionFactory(TestConf.FactFact, SqlServerVersion.v2008),
                    new SqlDbJobQueueDefaultTableConfiguration(), new NullOnMissingTypeJobTypeResolver());
            }
        }

        protected Func<IJobQueuePurger> jobQueuePurgerFunc
        {
            get
            {
                return () => new BaseSqlJobQueuePurger(new SqlServerDataConnectionFactory(TestConf.FactFact, SqlServerVersion.v2008),
                    new SqlDbJobQueueDefaultTableConfiguration());
            }
        }
    }
    [ExcludeFromCodeCoverage]
    public class PurgeTestJob
    {
        public static int getCounterValue(string param)
        {
            lock (_lockObject)
            {
                return counterDict.ContainsKey(param) ? counterDict[param] : 0;
            }
        }
        private static object _lockObject = new object();
        public static Dictionary<string, int> counterDict = new Dictionary<string, int>();
        public void JobMethod(string param)
        {
            lock (_lockObject)
            {
                if (counterDict.ContainsKey(param))
                {
                    counterDict[param] = counterDict[param] + 1;
                }
                else
                {
                    counterDict[param] = 1;
                }
            }
        }
    }
}