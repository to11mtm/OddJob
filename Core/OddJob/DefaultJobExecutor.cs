using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{

    public static class MethodInfoHelper
    {
        public static ConcurrentDictionary<int, MethodInfo> genericMethodInfoHash = new ConcurrentDictionary<int, MethodInfo>();
        public static ConcurrentDictionary<int, MethodInfo> nonGenericMethodInfoHash = new ConcurrentDictionary<int, MethodInfo>();
        public static MethodInfo GetMethodInfoForExpr(IOddJob expr)
        {
            var args = expr.JobArgs;
            MethodInfo method = null;
            if (expr.MethodGenericTypes != null &&
                expr.MethodGenericTypes.Length > 0)
            {
                //Generic Types are nasty when it comes to hash codes.
                //Because we are pivoting here,
                int hc = expr.MethodName.GetHashCode();
                hc = unchecked(hc * 31 + expr.TypeExecutedOn.GetHashCode());
                for (int i = 0; i < expr.MethodGenericTypes.Length; i++)
                {
                    hc = unchecked(hc * 31 +
                                   expr.MethodGenericTypes[i]
                                       .GetHashCode());
                }
                hc = unchecked(hc * 31 + args.Length);
                for (int i = 0; i < args.Length; i++)
                {
                    hc = unchecked(hc * 31 +
                                   args[i].Type.GetHashCode());
                }
                method = genericMethodInfoHash.GetOrAdd(hc,
                    i => createGenericMethod(expr));
                if (expr.TypeExecutedOn.IsAssignableFrom(
                    method.DeclaringType) == false)
                {
                    //Hash Collision. unlikely we'd get here, but let's be sure. :)
                    method = createGenericMethod(expr);
                }
                if (method == null)
                {
                    throw new ArgumentException(
                        $"Could not find generic method for Type {expr.TypeExecutedOn.Name}, Method {expr.MethodName}, Arity {expr.MethodGenericTypes.Length}");
                }
            }
            else
            {
                int hc = expr.MethodName.GetHashCode();
                for (int i = 0; i < args.Length; i++)
                {
                    hc = unchecked(hc * 31 + args[i].GetHashCode());
                }

                method = nonGenericMethodInfoHash.GetOrAdd(hc,
                    (h) => expr.TypeExecutedOn.GetMethod(expr.MethodName,
                        expr.JobArgs.Select(q => q.Value.GetType()).ToArray()));
                if (expr.TypeExecutedOn.IsAssignableFrom(
                    method.DeclaringType) == false)
                {
                    //Hash Collision. unlikely we'd get here, but let's be sure. :)
                    method = expr.TypeExecutedOn.GetMethod(expr.MethodName,
                        expr.JobArgs.Select(q => q.Value.GetType()).ToArray());
                }

                if (method == null)
                {
                    throw new ArgumentException($"Could not find method for Type {expr.TypeExecutedOn.Name}, Method {expr.MethodName}");
                }
            }

            return method;
        }

        private static MethodInfo createGenericMethod(IOddJob expr)
        {
            var methodToBuild = expr.TypeExecutedOn.GetMethods()
                .FirstOrDefault(q => q.Name == expr.MethodName &&
                                     q.IsGenericMethod &&
                                     q.GetGenericArguments().Length ==
                                     expr.MethodGenericTypes.Length &&
                                     q.GetParameters().Length == expr.JobArgs.Length);
            return methodToBuild.MakeGenericMethod(
                expr.MethodGenericTypes);
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

    /// <summary>
    /// Contains a Cache of built Functions
    /// </summary>
    /// <remarks>
    /// We use this class to help us cache our Delegates efficiently.
    ///  - Comparing MethodInfo is relatively slow for Generic calls
    ///  - Type comparisons are relatively cheap
    /// </remarks>
    public static class ExecutionCacheContainer
    {
        private static ConcurrentDictionary<Type, ConcurrentDictionary<int,
            Func<object, object[], object>>> cache =
            new ConcurrentDictionary<Type, ConcurrentDictionary<int,
                Func<object, object[], object>>>();

        public  static ConcurrentDictionary<int, Func<object, object[], object>>
            GetContainer(Type forType)
        {
            return cache.GetOrAdd(forType,
                type=> new ConcurrentDictionary<int, Func<object, object[], object>
                >());
        }
    }
    public class DefaultJobExecutor : IJobExecutor
    {
        public DefaultJobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        private IContainerFactory _containerFactory;
     
        /// <summary>
        /// Creates a Delegate to execute a given MethodInfo
        /// </summary>
        /// <param name="method">The Method to execute</param>
        /// <returns>A built Delegate</returns>
        private Func<object,object[],object> CreateExpr(MethodInfo method)
        {
            var args = method.GetParameters();
            Expression inInstance = null;
            // public object methodProxy(object instance, object[] inArgs)
            var instancePar = Expression.Parameter(typeof(object), "instance");
            var param = Expression.Parameter(typeof(object[]), "inArgs");
            Expression[] convArgs = new Expression[args.Length];
            // (inArgs[0],inArgs[1].....)
            for(int i=0; i<args.Length;i++)
            {
                convArgs[i] = Expression.Convert(Expression.ArrayAccess(param, Expression.Constant(i)), args[i].ParameterType);
            }
            Expression call = null;
            if (method.IsStatic && method.IsDefined(typeof(ExtensionAttribute), false) == false) 
            {
                // don't set an instance for a static/extension,
                // will throw if we do
            }
            else
            {
                inInstance = Expression.Convert(instancePar, method.DeclaringType);
            }
            if (method.ReturnType == typeof(void))
            {
                // If void, we return NULL.
                // Our calling convention ensures we still show VOID
                // When the call returns.
                call = Expression.Block(
                    Expression.Call(inInstance, method, convArgs),
                    Expression.Constant(null, typeof(object)));
            }
            else
            {
                // Do the call. Convert is relatively cheap when not needed,
                // So for now we don't worry about whether to box or not.
                call = Expression.Convert(Expression.Call(inInstance, method, convArgs), typeof(object));
            }
            return Expression.Lambda<Func<object, object[], object>>(call, instancePar, param).Compile();
        }
        public static bool UseBuiltExpressions = true;
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

            var args = expr.JobArgs;
            object result = null;
            if (UseBuiltExpressions)
            {
                var mc = ExecutionCacheContainer
                    .GetContainer(expr.TypeExecutedOn)
                    .GetOrAdd(method.GetHashCode(), (mi) => CreateExpr(method));
                result = mc(instance, GetValues(args));
            }
            else
            {
                result = method.Invoke(instance, GetValues(args));
            }
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
        //private ArrayPool<object> arrayPool = new ArrayPool<object>
        private static object[] GetValues(OddJobParameter[] args)
        {
            var objArr = new object[args.Length];
            for(int i=0; i<args.Length; i++)
            {
                objArr[i] = args[i].Value;
            }
            return objArr;
        }

    }
}