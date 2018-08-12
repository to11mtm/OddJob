using System.Data.SqlClient;
using GlutenFree.OddJob.Storage.SQL.Common;
using LinqToDB.Data;
using LinqToDB.Mapping;

namespace OddJob.Storage.Sql.SqlServer.Test
{
    public class TestConnectionFactory : IJobQueueDbConnectionFactory
    {
        public SqlConnection GetConnection()
        {
            return SqlConnectionHelper.GetLocalDB("unittestdb");
        }

        private JobQueueDbConnectionFactorySettings settings;

        public TestConnectionFactory(JobQueueDbConnectionFactorySettings settings)
        {
            this.settings = settings;
        }

        public DataConnection CreateDbConnection(MappingSchema mappingSchema)
        {
            return new DataConnection(settings.ProviderName, SqlConnectionHelper.CheckConnString("unittestdb"), mappingSchema);
        }
    }
}