using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace GlutenFree.Linq2Db.Helpers
{
    /// <summary>
    /// A Helper class to be used with Linq2Db in order to allow for parameterized 'like' queries to a DB.
    /// </summary>
    public class LikeAnyExpressionReplacer : ExpressionVisitor
    {
        //
        private static MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", types:new Type[]{typeof(string)});
        private static MethodInfo ToUpperMethod = typeof(string).GetMethod("ToUpper", types:new Type[]{});
        private static MethodInfo ToLowerMethod = typeof(string).GetMethod("ToLower",types:new Type[]{});
        protected Lazy<GetValuesExpressionReplacer> _getValuesExpressionReplacer = new Lazy<GetValuesExpressionReplacer>();
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "LikeAny" || node.Method.Name == "LikeAll"||
                node.Method.Name == "LikeAnyLower" || node.Method.Name == "LikeAllLower"||
                node.Method.Name == "LikeAnyUpper" || node.Method.Name == "LikeAllUpper")
            {
                bool asUpper = node.Method.Name.EndsWith("Upper");
                bool asLower = node.Method.Name.EndsWith("Lower");
                var arg = node.Arguments[0];
                var obj = node.Arguments[1];

                // obj will be null, if Contains is called for IQueryable. That is handled ok by default
                if (obj != null && arg is MemberExpression)
                {
                    IEnumerable values;

                    var argMember = asLower ? Expression.Call(arg,ToLowerMethod) :
                        asUpper ? Expression.Call(arg,ToUpperMethod) : arg;
                    if (obj is MemberExpression)
                    {
                        var objectMember = Expression.Convert(obj, typeof(object));

                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);

                        var getter = getterLambda.Compile();

                        values = getter() as IEnumerable;
                    }
                    // case for NewArrayExpression, InitListExpression and so on
                    // when used like: x => new List{...}.Contains(x.Id)
                    else
                    {
                        values = _getValuesExpressionReplacer.Value.GetValues(obj);
                    }

                    if (values != null)
                    {
                        BinaryExpression res = null;

                        foreach (object o in values)
                        {
                            var expressionScopedVariablesType = typeof(ExpressionScopedVariables<>).MakeGenericType(argMember.Type);

                            var scope = Activator.CreateInstance(expressionScopedVariablesType);

                            var getVariable = expressionScopedVariablesType.GetField("Value");
                            var newVal = asUpper ? o.ToString().ToUpper() : asLower ? o.ToString().ToLower() : o;
                            getVariable.SetValue(scope, newVal);

                            var scopeExp = Expression.Constant(scope);

                            var accessExpr = Expression.MakeMemberAccess(scopeExp, getVariable);
                            
                            


                            var equalsExpr = Expression.MakeBinary(ExpressionType.Equal, Expression.Call(argMember, ContainsMethod, accessExpr), Expression.Constant(true));

                            if (res == null)
                            {
                                res = equalsExpr;
                            }
                            else
                            {
                                if (node.Method.Name == "LikeAny")
                                {
                                    res = Expression.OrElse(res, equalsExpr);
                                }
                                else if (node.Method.Name == "LikeAll")
                                {
                                    res = Expression.AndAlso(res, equalsExpr);
                                }

                            }
                        }

                        // case when res is null (Contains is called for empty collection) is handled ok by default
                        if (res != null)
                        {
                            return res;
                        }
                    }

                }
            }

            return base.VisitMethodCall(node);
        }
    }
}