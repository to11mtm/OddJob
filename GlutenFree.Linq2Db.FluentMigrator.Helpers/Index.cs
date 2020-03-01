using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace GlutenFree.Linq2Db.FluentMigrator.Helpers
{
    public class Index<T>
    {
        public class IndexColExpr
        {
            public Expression<Func<T, object>> expr;
            public DirectionEnum dir;
        }
        
        
        public List<IndexColExpr> IndexColExprs { get; protected set; }
        public static Index<T> Create()
        {
            return new Index<T>();
        }
        public Index()
        {
            IndexColExprs= new List<IndexColExpr>();
        }
        public Index<T> Column(Expression<Func<T, object>> selectorExpression, DirectionEnum direction= DirectionEnum.unspecified)
        {
            IndexColExprs.Add(new IndexColExpr(){dir = direction, expr = selectorExpression});
            return this;
        }
    }
}