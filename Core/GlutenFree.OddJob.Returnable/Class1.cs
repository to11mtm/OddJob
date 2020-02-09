using System;
using System.Linq.Expressions;

namespace GlutenFree.OddJob.Returnable
{
    public class ReturnableOddJob : OddJob
    {
        public ReturnableOddJob(string methodName, OddJobParameter[] jobArgs, Type typeExecutedOn, Type[] methodGenericTypes, string status = JobStates.New) : base(methodName, jobArgs, typeExecutedOn, methodGenericTypes, status)
        {
        }
    }
    public static class ReturnableJobCreator
    {
        public static void Create<T, TReturn>(Expression<Func<T, TReturn>> jobExpr)
        {
            ExpressionBasedJobCreator.CreateInternal<T>(jobExpr.Body as MethodCallExpression);
        }
    }
}
