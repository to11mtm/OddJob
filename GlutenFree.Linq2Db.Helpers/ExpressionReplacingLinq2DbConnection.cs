using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.Linq;
using LinqToDB.Mapping;

namespace GlutenFree.Linq2Db.Helpers
{
    public class ExpressionReplacingLinq2DbConnection : DataConnection, IExpressionPreprocessor
    {
        protected List<ExpressionVisitor> customVisitors = new List<ExpressionVisitor>();

        private List<ExpressionVisitor> defaultVisitors = new List<ExpressionVisitor>()
            {new LikeAnyExpressionReplacer()};

        public ExpressionReplacingLinq2DbConnection(MappingSchema mappingSchema,List<ExpressionVisitor> customVisitors) : base(mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(string configurationString,List<ExpressionVisitor> customVisitors) : base(configurationString)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(string configurationString, MappingSchema mappingSchema, List<ExpressionVisitor> customVisitors) : base(
            configurationString, mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(string providerName, string connectionString, List<ExpressionVisitor> customVisitors) : base(providerName,
            connectionString)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, string connectionString, List<ExpressionVisitor> customVisitors) : base(
            dataProvider, connectionString)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, IDbConnection connection, List<ExpressionVisitor> customVisitors) : base(
            dataProvider, connection)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, IDbTransaction transaction, List<ExpressionVisitor> customVisitors) : base(
            dataProvider, transaction)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(string providerName, string connectionString,
            MappingSchema mappingSchema, List<ExpressionVisitor> customVisitors) : base(providerName, connectionString, mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, string connectionString,
            MappingSchema mappingSchema, List<ExpressionVisitor> customVisitors) : base(dataProvider, connectionString, mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, IDbConnection connection,
            MappingSchema mappingSchema, List<ExpressionVisitor> customVisitors) : base(dataProvider, connection, mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, IDbConnection connection,
            bool disposeConnection, List<ExpressionVisitor> customVisitors) : base(dataProvider, connection, disposeConnection)
        {
            this.customVisitors = customVisitors;
        }

        public ExpressionReplacingLinq2DbConnection(IDataProvider dataProvider, IDbTransaction transaction,
            MappingSchema mappingSchema, List<ExpressionVisitor> customVisitors) : base(dataProvider, transaction, mappingSchema)
        {
            this.customVisitors = customVisitors;
        }

        public Expression ProcessExpression(Expression expression)
        {
            var newExpr = expression;
            customVisitors.ForEach(v => { newExpr = v.Visit(newExpr); });
            defaultVisitors.ForEach(v => { newExpr = v.Visit(newExpr); });
            return newExpr;
        }
    }
}