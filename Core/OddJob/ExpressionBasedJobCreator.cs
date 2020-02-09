using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private static bool UseFastExpressionCompiler { get; set; } = false;

        private static readonly ConcurrentDictionary<MethodInfo, string[]>
            paramNameDictionary =
                new ConcurrentDictionary<MethodInfo, string[]>();

        private static readonly Type _staticClassType = typeof(StaticClassJob);
        private static readonly Type _objectType = typeof(object);
        private static readonly Type _guidType = typeof(Guid);
        public static OddJob CreateInternal<T>(MethodCallExpression methodCall, ParameterExpression parameterExpression = null)
        {
            var jobGuid = Guid.NewGuid();
            var TypeExecutedOn = typeof(T); //jobExpr.Parameters[0].Type;


            var argProv = methodCall.Arguments;
            //Kinda Hacky: If the Caller uses OBJECT as the type OR our special StaticJob Class,
            //we get the type from the expression methodcall itself
            if (TypeExecutedOn == _objectType ||
                TypeExecutedOn == _staticClassType)
            {
                TypeExecutedOn = methodCall.Method.DeclaringType;
            }

            var argCount = argProv.Count;
            
            string[] paramNames;

            var methodInfo = methodCall.Method;
            paramNames = paramNameDictionary.GetOrAdd(methodInfo,
                (mi) => mi.GetParameters().Select(r => r.Name).ToArray());

            var _jobArgs = ParseJobArgs<T>(parameterExpression, argCount, argProv, jobGuid, paramNames);


            var genericArgs = methodInfo.GetGenericArguments();
            return new OddJob(jobGuid, methodInfo.Name, _jobArgs,
                TypeExecutedOn, genericArgs);
        }

        private static OddJobParameter[] ParseJobArgs<T>(
            ParameterExpression parameterExpression, int argCount,
            ReadOnlyCollection<Expression> argProv, Guid jobGuid, string[] paramNames)
        {
            OddJobParameter[] _jobArgs = new OddJobParameter[argCount];
            for (int i = 0; i < argCount; i++)
            {
                var theArg = argProv[i];
                object val = null;
                try
                {
                    if (theArg is ConstantExpression _theConst)
                    {
                        //If constant, no need for invokes.
                        val = _theConst.Value;
                    }
                    else if ((theArg as ParameterExpression)?.Type ==
                             _guidType)
                    {
                        //Use the Job Guid.
                        val = jobGuid;
                    }
                    else
                    {
                        //If we are dealing with a Valuetype, we need a convert.
                        var convArg = ConvertIfNeeded(theArg);

                        //Easy version: instead of trying to walk the tree
                        //And see if we are in a state needing conversion,
                        //We just pass guid every time if that overload was
                        //called.
                        bool memSet = false;
                        if (theArg is MemberExpression)
                        {
                            if (theArg is MemberExpression _memArg)
                            {
                                if (_memArg.Expression is ConstantExpression c)
                                {
                                    if (_memArg.Member is FieldInfo f)
                                    {
                                        val = f.GetValue(c.Value);
                                        memSet = true;
                                    }
                                    else if (_memArg.Member is PropertyInfo p)
                                    {
                                        val = p.GetValue(p);
                                        memSet = true;
                                    }
                                }
                            }
                        }
                        if (memSet == false)
                        {
                            if (parameterExpression != null)
                            {
                                val = CompileExprWithConvert(Expression
                                        .Lambda<Func<Guid, object>>(
                                            convArg,
                                            parameterExpression))
                                    .Invoke(jobGuid);
                            }
                            else
                            {
                                if (memSet == false)
                                {
                                    val = CompileExprWithConvert(Expression
                                            .Lambda<Func<object>>(
                                                convArg))
                                        .Invoke();
                                }
                            }
                        }
                    }

                    _jobArgs[i] = new OddJobParameter()
                    {
                        Name = paramNames[i],
                        Value = val
                    };
                }
                catch (Exception exception)
                {
                    //Fallback. Do the worst way.
                    try
                    {
                        object fallbackVal;
                        if (parameterExpression == null)
                        {
                            fallbackVal = Expression.Lambda(
                                    Expression.Convert(theArg, _objectType)
                                )
                                .Compile().DynamicInvoke();
                        }
                        else
                        {
                            //Expression.Lambda()
                            fallbackVal = Expression.Lambda(
                                    Expression.Convert(theArg, _objectType),
                                    parameterExpression)
                                .Compile().DynamicInvoke(jobGuid);
                        }

                        _jobArgs[i] = new OddJobParameter()
                        {
                            Name = paramNames[i],
                            Value = fallbackVal
                        };
                    }

                    catch (Exception ex)
                    {
                        throw new JobCreationException(
                            "Couldn't derive value from job! Please use variables whenever possible and avoid methods that take parameters",
                            ex);
                    }
                }
            }

            return _jobArgs;
        }
        
        private static ConcurrentDictionary<Type,Type> GuidTypeDict { get; }= new ConcurrentDictionary<Type, Type>();
        private static ConcurrentDictionary<Type,Type> NormalTypeDict { get; }= new ConcurrentDictionary<Type, Type>();

        public static Expression ConvertIfNeeded(Expression toConv)
        {
            Type retType = null;
            if (toConv.NodeType == ExpressionType.Lambda)
            {
                retType = TraverseForType(toConv.Type.GetGenericArguments()
                    .LastOrDefault());
            }
            else
            {
                retType = toConv.Type;
            }

            if (retType?.BaseType == _objectType)
            {
                return toConv;
            }
            else
            {
                return Expression.Convert(toConv, _objectType);
            }
        }

        public static Type TraverseForType(Type toConv)
        {
            if (toConv == null)
            {
                return null;
            }
            else if (toConv == typeof(MulticastDelegate))
            {
                //I don't think this should happen in sane usage, but let's cover it.
                return (TraverseForType(toConv.GetGenericArguments().LastOrDefault()));
            }
            else
            {
                return toConv.GetType();
            }
        }
        private static T CompileExprWithConvert<T>(Expression<T> lambda) where T : class
        {
            if (UseFastExpressionCompiler)
            {
                return FastExpressionCompiler.ExpressionCompiler.CompileFast(
                    lambda);
            }
            else
            {
                return lambda.Compile();
            }
        }
    }
}