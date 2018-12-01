using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            return ExpressionBasedJobCreator.CreateInternal<T>(jobExpr.Body as MethodCallExpression);
        }
        
    }
}