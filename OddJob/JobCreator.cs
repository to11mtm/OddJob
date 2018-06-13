using System;
using System.Linq.Expressions;

namespace OddJob
{
    public static class JobCreator
    {
        public static OddJob Create<T>(Expression<Action<T>> jobExpr)
        {
            var _jobExpr = jobExpr;
            var TypeExecutedOn = typeof(T);//jobExpr.Parameters[0].Type;

            var argProv = (_jobExpr.Body as MethodCallExpression).Arguments;

            var argCount = argProv.Count;
            object[] _jobArgs = new object[argCount];
            for (int i = 0; i < argCount; i++)
            {
                var theArg = argProv[i];
                if (theArg is ConstantExpression)
                {
                    _jobArgs[i] = (theArg as ConstantExpression).Value;
                }
                else if (theArg is MemberExpression)
                {
                    _jobArgs[i] = Expression.Lambda(theArg).Compile().DynamicInvoke();
                }
                else if (theArg is MemberInitExpression)
                {
                    _jobArgs[i] = Expression.Lambda(theArg).Compile().DynamicInvoke();
                }
         
            }
            var moarInfo = ((MethodCallExpression)_jobExpr.Body).Method.Name;
            var methodName = typeof(T).GetMethod(moarInfo);
            return new OddJob(methodName.Name, _jobArgs, TypeExecutedOn);
        }
    }
}