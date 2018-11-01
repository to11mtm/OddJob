using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace GlutenFree.OddJob
{
    public class JobCreationException : Exception
    {
        public JobCreationException()
        {
        }

        public JobCreationException(string message) : base(message)
        {
        }

        public JobCreationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected JobCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    /// <summary>
    /// Optional way to pass Jobs that are Static method calls (i.e. calls in a Static Class)
    /// </summary>
    public class StaticClassJob
    {

    }

    

    public static class JobCreator
    {
        public static Dictionary<MethodInfo, string[]> paramNameDictionary =
            new Dictionary<MethodInfo, string[]>();
        public static OddJob Create<T>(Expression<Action<T>> jobExpr)
        {
            var _jobExpr = jobExpr;
            var TypeExecutedOn = typeof(T);//jobExpr.Parameters[0].Type;
            
            var methodCall = (_jobExpr.Body as MethodCallExpression);
            var argProv = methodCall.Arguments;
            //Kinda Hacky: If the Caller uses OBJECT as the type OR our special StaticJob Class,
            //we get the type from the expression methodcall itself
            if (TypeExecutedOn==typeof(StaticClassJob) ||TypeExecutedOn == typeof(object))
            {
                TypeExecutedOn = methodCall.Method.DeclaringType;
            }
            var argCount = argProv.Count;
            var methodInfo = ((MethodCallExpression)_jobExpr.Body).Method;
            string[] paramNames;
            if (paramNameDictionary.ContainsKey(methodInfo) == false)
            {
                paramNames = paramNameDictionary[methodInfo] = methodInfo.GetParameters().Select(q=>q.Name).ToArray();
            }
            else
            {
                paramNames = paramNameDictionary[methodInfo];
            }
            
            OddJobParameter[] _jobArgs = new OddJobParameter[argCount];
            for (int i = 0; i < argCount; i++)
            {

                var theArg = argProv[i];
                
                try
                {
                    if (theArg is ConstantExpression)
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i], Value = ((ConstantExpression) theArg).Value
                        };
                    }
                    else if (theArg is MemberExpression)
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i], Value = GetMemberValue((MemberExpression) theArg)
                        };
                    }
                    else
                    {
                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i], Value = Expression.Lambda(theArg).Compile().DynamicInvoke()
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