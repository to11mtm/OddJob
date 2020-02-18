using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{

    public static class MethodInfoHelper
    {
        public static MethodInfo GetMethodInfoForExpr(IOddJob expr)
        {
            MethodInfo method;
            if (expr.MethodGenericTypes != null && expr.MethodGenericTypes.Length > 0)
            {
                method = expr.TypeExecutedOn.GetMethods()
                    .FirstOrDefault(q => q.Name == expr.MethodName &&
                                         q.IsGenericMethod && q.GetGenericArguments().Length ==
                                         expr.MethodGenericTypes.Length &&
                                         q.GetParameters().Length == expr.JobArgs.Length)
                    ?.MakeGenericMethod(expr.MethodGenericTypes);
                if (method == null)
                {
                    throw new ArgumentException($"Could not find generic method for Type {expr.TypeExecutedOn.Name}, Method {expr.MethodName}, Arity {expr.MethodGenericTypes.Length}");
                }
            }
            else
            {
                method = expr.TypeExecutedOn.GetMethod(expr.MethodName,
                    expr.JobArgs.Select(q => q.Value.GetType()).ToArray());
                if (method == null)
                {
                    throw new ArgumentException($"Could not find method for Type {expr.TypeExecutedOn.Name}, Method {expr.MethodName}");
                }
            }

            return method;
        }
    }

    public class OddJobResult : IOddJobResult
    {
        public object Result { get; set; }
        public Type ReturnType { get; set; }
    }

    public interface IOddJobResult
    {
        object Result { get; set; }
        Type ReturnType { get; set; }
    }
    
    public class DefaultJobExecutor : IJobExecutor
    {
        public DefaultJobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        private IContainerFactory _containerFactory;

        public IOddJobResult ExecuteJob(IOddJob expr)
        {
            //IsAbstract and IsSealed means we are dealing with a static class invocation and want NULL.
            var instance = (expr.TypeExecutedOn.IsAbstract && expr.TypeExecutedOn.IsSealed)
                    ? null
                    : _containerFactory.CreateInstance(expr.TypeExecutedOn)
                ;
            MethodInfo method = null;
            
            method = MethodInfoHelper.GetMethodInfoForExpr(expr);
            
            //var method = expr.TypeExecutedOn.GetMethod(expr.MethodName, expr.JobArgs.Select(q=>q.Value.GetType()).ToArray());
            var result = method.Invoke(instance, expr.JobArgs.Select(q=>q.Value).ToArray());
            _containerFactory.Release(instance);
            if (method.ReturnType != typeof(void))
            {
                return new OddJobResult() {Result = result, ReturnType = method.ReturnType};
            }
            else
            {
                return new OddJobResult() {Result = result, ReturnType = method.ReturnType};
            }
        }

        
    }
}