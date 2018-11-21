using System;
using System.Linq;
using System.Linq.Expressions;
using Akka.Actor;
using Akka.Dispatch;
using Akka.Event;

namespace GlutenFree.OddJob.Execution.Akka
{
    public class HardInjectedJobExecutorShell : BaseJobExecutorShell
    {
        public HardInjectedJobExecutorShell(Expression<Func<JobQueueLayerActor>> jobQueueFunc,
            Expression<Func<JobWorkerActor>> workerFunc,
            Expression<Func<JobQueueCoordinator>> coordinatorFunc,
            IExecutionEngineLoggerConfig loggerConfig) : base(loggerConfig)
        {
            JobQueueProps = Props.Create(jobQueueFunc);
            WorkerProps = Props.Create(workerFunc);
            CoordinatorProps = Props.Create(coordinatorFunc);
        }


        protected override Props WorkerProps { get; }
        protected override Props JobQueueProps { get; }
        protected override Props CoordinatorProps { get; }
    }

    public static class ParameterReplacer
    {
        // Produces an expression identical to 'expression'
        // except with 'source' parameter replaced with 'target' expression.     
        public static Expression<TOutput> Replace<TInput, TOutput>
        (Expression<TInput> expression,
            ParameterExpression source,
            Expression target)
        {
            return new ParameterReplacerVisitor<TOutput>(source, target)
                .VisitAndConvert(expression);
        }

        private class ParameterReplacerVisitor<TOutput> : ExpressionVisitor
        {
            private ParameterExpression _source;
            private Expression _target;

            public ParameterReplacerVisitor
                (ParameterExpression source, Expression target)
            {
                _source = source;
                _target = target;
            }

            internal Expression<TOutput> VisitAndConvert<T>(Expression<T> root)
            {
                return (Expression<TOutput>)VisitLambda(root);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                // Leave all parameters alone except the one we want to replace.
                var parameters = node.Parameters
                    .Where(p => p.Type != typeof(ConstructingProps));
                
                return Expression.Lambda<TOutput>(Visit(node.Body), parameters);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // Replace the source with the target, visit other params as usual.
                return node.Type == _source.Type ? _target : base.VisitParameter(node);
            }
        }
    }

    public class ConstructingProps
    {
        public Props WorkerProps { get; set; }
        public Props JobQueueProps { get; set; }
    }
}