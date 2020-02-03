using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.SQLite.Test;
using GlutenFree.OddJob.Tests;
using Xunit;

namespace GlutenFree.OddJob.PerfTests
{
    public class Perftest1
    {
        public string Perftest_Sqlite_Store(int iters)
        {
            UnitTestTableHelper.EnsureTablesExist();
            var jobWriter = new SQLiteJobQueueAdder(
                new SQLiteJobQueueDataConnectionFactory(UnitTestTableHelper
                    .connString),
                new DefaultJobAdderQueueTableResolver(
                    new SqlDbJobQueueDefaultTableConfiguration()));
            var myvalue = new ClassTest()
                {classTestValue = TestConstants.classTestValue};
            var next =
                SerializableJobCreator.CreateJobDefinition<SampleJobInGenericClass<string>>(
                    j =>
                        j.DoThing(TestConstants.derp,
                            TestConstants.herp, myvalue));

            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            for (int i = 0; i < iters; i++)
            {
                next.JobId = Guid.NewGuid();
                jobWriter.AddJob(next);
            }
            sw1.Stop();
            return sw1.Elapsed.TotalSeconds.ToString();
        }
        [InlineData(new object[]{4,50000})]
        [Theory]
        public string PerfTest_MultiThread_Serialized(int threads, int iters)
        {
            return PerfTest_MultiThread(threads, iters, () =>
            {
                for (int i = 0; i < iters; i++)
                {
                    var myvalue = new ClassTest()
                        {classTestValue = TestConstants.classTestValue};
                    var next =
                        SerializableJobCreator.CreateJobDefinition<SampleJobInGenericClass<string>>(
                                j =>
                                    j.DoThing(TestConstants.derp,
                                        TestConstants.herp, myvalue));
                }
            });
        }
        [InlineData(50000)]
        [Theory]
        public string JobSerialzierPerfTestSlow(int iters)
        {
            

            var sw2 = Stopwatch.StartNew();
            for (int i = 0; i < iters; i++)
            {
                    
                var next = SerializableJobCreator.CreateJobDefinition<SampleJobInGenericClass<string>>(j =>
                    j.DoThing(TestConstants.derp, TestConstants.herp,  new ClassTest() { classTestValue = TestConstants.classTestValue }));
            }
            sw2.Stop();

            return $"Iterations {iters} - New/Members - {sw2.Elapsed.TotalSeconds}, ops/Sec - {(double)iters/sw2.Elapsed.TotalSeconds}";
            
        }
        [InlineData(50000)]
        [Theory]
        public string JobSerialzierPerfTest_Fast(int iters)
        {
            
                var sw1  = Stopwatch.StartNew();
                for (int i = 0; i < iters; i++)
                {
                    var myvalue = new ClassTest() { classTestValue = TestConstants.classTestValue };
                    var next = SerializableJobCreator.CreateJobDefinition<SampleJobInGenericClass<string>>(j =>
                        j.DoThing(TestConstants.derp, TestConstants.herp, myvalue));
                }
                sw1.Stop();

                return $"Iterations {iters} - Values/Members - {sw1.Elapsed.TotalSeconds}, ops/Sec - {(double)iters/sw1.Elapsed.TotalSeconds}";
            
        }
        
        [InlineData(50000)]
        [Theory]
            public string PerfTest(int iters)
            {
                var sw1  = Stopwatch.StartNew();
                for (int i = 0; i < iters; i++)
                {
                    var myvalue = new ClassTest() { classTestValue = TestConstants.classTestValue };
                    var next = JobCreator.Create<SampleJobInGenericClass<string>>(j =>
                        j.DoThing(TestConstants.derp, TestConstants.herp, myvalue));
                }
                sw1.Stop();
                return sw1.Elapsed.TotalSeconds.ToString();
            }
            
            [InlineData(new object[]{4,50000})]
            [Theory]
            public string PerfTest_MultiThread_Base(int threads, int iters)
            {
                return PerfTest_MultiThread(threads, iters, () =>
                {
                    for (int i = 0; i < iters; i++)
                    {
                        var myvalue = new ClassTest()
                            {classTestValue = TestConstants.classTestValue};
                        var next =
                            JobCreator
                                .Create<SampleJobInGenericClass<string>>(
                                    j =>
                                        j.DoThing(TestConstants.derp,
                                            TestConstants.herp, myvalue));
                    }
                });
            }
            private string PerfTest_MultiThread(int threads, int iters,Action act)
            {
                Console.WriteLine($"Creating {threads} threads, {iters} iterations per thread");
                var taskList = new Thread[threads];
                for (int i = 0; i < threads; i++)
                {
                    taskList[i] = new Thread(new ThreadStart(act));
                }
                Console.WriteLine("Threads Created. Starting..." +
                                  "");
                var sw1  = Stopwatch.StartNew();
                for (int i = 0; i < threads; i++)
                {
                    taskList[i].Start();
                }
                
                foreach (var thread in taskList)
                {
                    thread.Join();
                }
                
                sw1.Stop();
                return sw1.Elapsed.TotalSeconds.ToString();
            }
        
    }
}