using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Akka.DI.SimpleInjector;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;
using GlutenFree.OddJob;
using GlutenFree.OddJob.Execution.Akka;
using GlutenFree.OddJob.Execution.Akka.Test;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.SQLite;
using GlutenFree.OddJob.Storage.Sql.SQLite.Test;
using GlutenFree.OddJob.Storage.Sql.SqlServer;
using GlutenFree.OddJob.Storage.Sql.TableHelper;
using LinqToDB.DataProvider.SqlServer;
using OddJob.Rpc.Execution.Plugin;
using OddJob.Storage.Sql.SqlServer.Test;

namespace Oddjob.Rpc.Redis.IntegrationTests
{
    public static class AsyncHelpers
    {
        public static Task ForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> action,
            int maxDegreesOfParallelism)
        {
            var actionBlock = new ActionBlock<TSource>(action, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreesOfParallelism
            });

            foreach (var item in items)
            {
                actionBlock.Post(item);
            }

            actionBlock.Complete();

            return actionBlock.Completion;
        }
    }
}