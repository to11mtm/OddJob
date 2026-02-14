using GlutenFree.OddJob.Storage.Sql.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    class SQLiteSampleHelper
    {
        public static SQLiteJobQueueDataConnectionFactory ConnFactoryFunc()
        {
            return new GlutenFree.OddJob.Storage.Sql.SQLite.SQLiteJobQueueDataConnectionFactory(SampleTableHelper.connString);
        }
    }
}