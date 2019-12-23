using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using FluentMigrator;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using GlutenFree.OddJob.Storage.Sql.Common;
using GlutenFree.OddJob.Storage.Sql.Common.DbDtos;

namespace GlutenFree.OddJob.Storage.Sql.TableHelper
{
    public class test : Migration
    {
        public override void Up()
        {
            Create.Index().
            Create.Table()
        }

        public override void Down()
        {
         
        }
    }

    public static class Helpers
    {
        public static string GetNameOfSelector<T>(Expression<Func<T,object>> selectorExpression)
        {
            return ((MemberExpression) selectorExpression.Body).Member.Name;
        }
    }
    public class Class1
    {
        public void Up(ISqlDbJobQueueTableConfiguration tableConfiguration)
        {
            
            var queueTableIndices = new[]
            {
                new CreateIndexExpression().Index = new IndexDefinition()
                {
                    TableName = tableConfiguration.QueueTableName,
                    Columns = new List<IndexColumnDefinition>()
                    {
                        new IndexColumnDefinition()
                            {Name = Helpers.GetNameOfSelector<SqlCommonDbOddJobMetaData>(t => t.LockGuid)}
                    },
                    IsUnique = true
                },
                new CreateIndexExpression().Index = new IndexDefinition()
                {
                    TableName = tableConfiguration.QueueTableName,
                    Columns = new List<IndexColumnDefinition>()
                    {
                        new IndexColumnDefinition()
                        {
                            Name = Helpers.GetNameOfSelector<SqlCommonDbOddJobMetaData>(t => t.QueueName)
                        },
                        new IndexColumnDefinition()
                        {
                            Name = Helpers.GetNameOfSelector<SqlCommonDbOddJobMetaData>(t => t.Status)
                        }
                    }
                },

                new CreateIndexExpression().Index = new IndexDefinition()
                {
                    TableName = tableConfiguration.QueueTableName,
                    Columns = new List<IndexColumnDefinition>()
                    {
                        new IndexColumnDefinition()
                            {Name = Helpers.GetNameOfSelector<SqlCommonDbOddJobMetaData>(t => t.LockGuid)}
                    },
                    IsUnique = true
                }

            };
            var gen = new FluentMigrator.Runner.Generators.SQLite.SQLiteGenerator();
            gen.Generate()
            
        }

    }
}
