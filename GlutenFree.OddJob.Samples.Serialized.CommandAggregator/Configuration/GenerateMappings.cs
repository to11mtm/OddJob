using System.Collections.Generic;
using GlutenFree.OddJob.Storage.SQL.Common;

namespace GlutenFree.OddJob.Samples.Serialized.CommandAggregator
{
    /// <summary>
    /// This is a helper class to provide mappings for Queuenames to specific tables.
    /// If you want to use a single Queue table, you do not need to do this.
    /// But for scenarios where you wish to have multiple queue tables,
    /// This is an opportunity to provide a level of indirection for serialized jobs.
    /// </summary>
    public static class GenerateMappings
    {
        public static Dictionary<string,ISqlDbJobQueueTableConfiguration> TableConfigurations
        {
            get
            {
                return new Dictionary<string, ISqlDbJobQueueTableConfiguration>()
                {
                    {
                        "console",
                        new MyTableConfigs()
                        {
                            QueueTableName = "consoleQueue", ParamTableName = "consoleParam",
                            JobMethodGenericParamTableName = "consoleGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    },
                    { "counter",
                        new MyTableConfigs()
                        {
                            QueueTableName = "counterQueue", ParamTableName = "counterParam",
                            JobMethodGenericParamTableName = "counterGeneric", JobClaimLockTimeoutInSeconds = 30
                        }
                    }
                };
            }
        }
    }
}