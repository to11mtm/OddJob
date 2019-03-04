using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GlutenFree.OddJob
{
    /// <summary>
    /// A JobCreator Designed to Produce Jobs from <see cref="MethodCallExpression"/> Expressions.
    /// </summary>
    public static class ExpressionBasedJobCreator
    {
        private static ConcurrentDictionary<MethodInfo, string[]> paramNameDictionary = new ConcurrentDictionary<MethodInfo, string[]>();

        public static OddJob CreateInternal<T>(MethodCallExpression methodCall)
        {

            var TypeExecutedOn = typeof(T);//jobExpr.Parameters[0].Type;


            var argProv = methodCall.Arguments;
            //Kinda Hacky: If the Caller uses OBJECT as the type OR our special StaticJob Class,
            //we get the type from the expression methodcall itself
            if (TypeExecutedOn == typeof(StaticClassJob) || TypeExecutedOn == typeof(object))
            {
                TypeExecutedOn = methodCall.Method.DeclaringType;
            }
            var argCount = argProv.Count;
            OddJobParameter[] _jobArgs = new OddJobParameter[argCount];
            string[] paramNames;

            var methodInfo = methodCall.Method;
            if (paramNameDictionary.ContainsKey(methodInfo) == false)
            {
                var methodParams = methodInfo.GetParameters().Select(q => q.Name).ToArray();
                paramNames = paramNameDictionary[methodInfo] = methodParams;
            }
            else
            {
                paramNames = paramNameDictionary[methodInfo];
            }
            for (int i = 0; i < argCount; i++)
            {

                var theArg = argProv[i];

                try
                {
                    if (theArg is ConstantExpression)
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i],
                            Value = ((ConstantExpression)theArg).Value
                        };
                    }
                    else if (theArg is MemberExpression)
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i],
                            Value = GetMemberValue((MemberExpression)theArg)
                        };
                    }
                    else
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i],
                            Value = Expression.Lambda(theArg).Compile().DynamicInvoke()
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new JobCreationException(
                        "Couldn't derive value from job! Please use variables whenever possible and avoid methods that take parameters",
                        ex);
                }



            }



            var genericArgs = methodInfo.GetGenericArguments();
            return new OddJob(methodInfo.Name, _jobArgs, TypeExecutedOn, genericArgs);
        }

        private static object GetMemberValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}