using System;
using System.Linq;
using System.Linq.Expressions;
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
            object[] _jobArgs = new object[argCount];
            for (int i = 0; i < argCount; i++)
            {

                var theArg = argProv[i];

                try
                {
                    _jobArgs[i] = Expression.Lambda(theArg).Compile().DynamicInvoke();
                }
                catch (Exception ex)
                {
                    throw new JobCreationException(
                        "Couldn't derive value from job! Please use variables whenever possible and avoid methods that take parameters",
                        ex);
                }



            }

            var methodInfo = ((MethodCallExpression) _jobExpr.Body).Method;
            var args = methodInfo.GetGenericArguments();
            return new OddJob(methodInfo.Name, _jobArgs, TypeExecutedOn, args);
        }
    }
}