using Akka.TestKit.Xunit2;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Serializable;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;

namespace GlutenFree.OddJob.Execution.Akka.Test
{
    public class AkkaExecutionTest : TestKit
    {


        public AkkaExecutionTest()
        {
            AkkaTestUnitTestTableHelper.EnsureTablesExist();
        }

        public static IJobQueueManager GetJobQueueManager
        {
            get
            {
                return new SQLiteJobQueueManager(
                    new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString),
                    new SqlDbJobQueueDefaultTableConfiguration(), new NullOnMissingTypeJobTypeResolver());
            }
        }

        public static IJobQueueAdder GetJobQueueAdder
        {
            get
            {
                return new SQLiteJobQueueAdder(new SQLiteJobQueueDataConnectionFactory(AkkaTestUnitTestTableHelper.connString),
                    new DefaultJobAdderQueueTableResolver(new SqlDbJobQueueDefaultTableConfiguration()));
            }
        }
    }
}