using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GlutenFree.Linq2Db.FluentMigrator.Helpers
{
    public class IndexCol<T>
    {
        
        public DirectionEnum Direction { get; protected set; }
        public string ColumnName { get; protected set; }
        public static IndexCol<T> Create(Expression<Func<T, object>> selectorExpression, DirectionEnum direction = DirectionEnum.unspecified)
        {
            MemberInfo colMember;
            if (selectorExpression.Body is UnaryExpression)
            {
                colMember = (((UnaryExpression) selectorExpression.Body).Operand as MemberExpression).Member;
            }
            else
            {
                colMember = ((MemberExpression) selectorExpression.Body).Member;
            }
            
            return new IndexCol<T>(colMember.Name, DirectionEnum.unspecified);
        }
        private IndexCol(string columnName, DirectionEnum direction)
        {
            Direction = direction;
            ColumnName = columnName;
        }
    }
    public static class IndexCol
    {
        public static IndexCol<T> Create<T>(Expression<Func<T, object>> selectorExpression)
        {
            return IndexCol<T>.Create(selectorExpression);
        }
    }
}