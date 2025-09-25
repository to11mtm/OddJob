using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
        public static OddJob Create<T>(LambdaExpression jobExpr)
        => CreateImpl<T>(jobExpr);
        public static OddJob CreateImpl<T>(LambdaExpression jobExpr)
        {
            return ExpressionBasedJobCreator.CreateInternal<T>(jobExpr.Body as MethodCallExpression);
        }
        public static OddJob Create<T>(Expression<Action<T>> jobExpr)
            => CreateImpl<T>(jobExpr as LambdaExpression);
        
        public static OddJob Create<T>(Expression<Func<T, Task>> jobExpr)
        => CreateImpl<T>(jobExpr);
        public static OddJob Create<T,TResult>(Expression<Func<T, Task<TResult>>> jobExpr)
        => CreateImpl<T>(jobExpr);

        public static OddJob Create<T>(Expression<Func<T, ValueTask>> jobExpr)
        => CreateImpl<T>(jobExpr);
        
        public static OddJob Create<T,TResult>(Expression<Func<T, ValueTask<TResult>>> jobExpr)
        => CreateImpl<T>(jobExpr);

        public static OddJob Create<T>(Expression<Action<Guid, T>> jobExpr)
        {
            return ExpressionBasedJobCreator.CreateInternal<T>(jobExpr.Body as MethodCallExpression, jobExpr.Parameters.First());
        }
        
    }
}