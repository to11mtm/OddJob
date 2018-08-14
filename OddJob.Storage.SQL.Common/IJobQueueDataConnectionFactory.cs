using System.Data.Common;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace GlutenFree.OddJob.Storage.SQL.Common
{
    public interface IJobQueueDataConnectionFactory
    {
        DataConnection CreateDataConnection(MappingSchema mappingSchema);
    }

    public interface IJobQueueDbConnectionFactory
    {
        DbConnection CreateDbConnection();
    }

    
}
