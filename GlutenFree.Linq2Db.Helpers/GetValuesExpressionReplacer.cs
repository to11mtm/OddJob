using System.Collections.Generic;
using System.Linq.Expressions;

namespace GlutenFree.Linq2Db.Helpers
{
    public class GetValuesExpressionReplacer : ExpressionVisitor
    {
        protected List<object> _values;

        public List<object> GetValues(Expression expr)
        {
            _values = new List<object>();
            Visit(expr);
            return _values;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            _values.Add(node.Value);
            return base.VisitConstant(node);
        }
    }
}