﻿using System.Linq;
using System.Reflection;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
    public class DefaultJobExecutor : IJobExecutor
    {
        public DefaultJobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        private IContainerFactory _containerFactory;

        public void ExecuteJob(IOddJob expr)
        {
            //IsAbstract and IsSealed means we are dealing with a static class invocation and want NULL.
            var instance = (expr.TypeExecutedOn.IsAbstract && expr.TypeExecutedOn.IsSealed)
                    ? null
                    : _containerFactory.CreateInstance(expr.TypeExecutedOn)
                ;
            MethodInfo method = null;
            if (expr.MethodGenericTypes != null && expr.MethodGenericTypes.Length > 0)
            {
                method = expr.TypeExecutedOn.GetMethods().Where(q =>
                        q.Name==expr.MethodName &&
                        q.IsGenericMethod && q.GetGenericArguments().Length == expr.MethodGenericTypes.Length &&
                        q.GetParameters().Length == expr.JobArgs.Length)
                    .FirstOrDefault().MakeGenericMethod(expr.MethodGenericTypes);
            }
            else
            {
                method = expr.TypeExecutedOn.GetMethod(expr.MethodName, expr.JobArgs.Select(q => q.Value.GetType()).ToArray());
            }
            //var method = expr.TypeExecutedOn.GetMethod(expr.MethodName, expr.JobArgs.Select(q=>q.Value.GetType()).ToArray());
            method.Invoke(instance, expr.JobArgs.Select(q=>q.Value).ToArray());
            _containerFactory.Relase(instance);
        }
    }
}