using GlutenFree.OddJob.Storage.SQL.SQLite;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    class SQLiteSampleHelper
    {
        public static SQLiteJobQueueDataConnectionFactory ConnFactoryFunc()
        {
            return new GlutenFree.OddJob.Storage.SQL.SQLite.SQLiteJobQueueDataConnectionFactory(SampleTableHelper.connString);
        }
    }
}