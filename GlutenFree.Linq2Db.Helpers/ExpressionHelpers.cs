using System;
using System.Linq.Expressions;

namespace GlutenFree.Linq2Db.Helpers
{
    //TODO: This is good for lots of things, not just Linq2DB! It should be in it's own library.
    public static class ExpressionHelpers
    {
        public static Expression<Func<T, bool>> CombineBinaryExpression<T>(Expression<Func<T, bool>> expr,
            Expression<Func<T, bool>> toAdd, bool trueAnd)
        {
            if (expr == null)
            {
                return toAdd;
            }
            else
            {
                if (trueAnd)
                {
                    return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(
                            new SwapVisitor(expr.Parameters[0], toAdd.Parameters[0]).Visit(expr.Body), toAdd.Body),
                        toAdd.Parameters);

                }
                else
                {
                    return Expression.Lambda<Func<T, bool>>(Expression.OrElse(
                            new SwapVisitor(expr.Parameters[0], toAdd.Parameters[0]).Visit(expr.Body), toAdd.Body),
                        toAdd.Parameters);
                }
            }
        }
    }
}