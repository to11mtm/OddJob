using System.Linq;

namespace OddJob
{
    public interface IJobExecutor
    {
        void ExecuteJob(IOddJob job);
    }

    public class DefaultJobExecutor : IJobExecutor
    {
        public DefaultJobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }

        private IContainerFactory _containerFactory;

        public void ExecuteJob(IOddJob expr)
        {
            var instance = _containerFactory.CreateInstance(expr.TypeExecutedOn);
            var method = expr.TypeExecutedOn.GetMethod(expr.MethodName, expr.JobArgs.Select(q=>q.GetType()).ToArray());
            method.Invoke(instance, expr.JobArgs);
        }
    }
    public class OldDefaultJobExecutor : IJobExecutor
    {
        public OldDefaultJobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }
        private IContainerFactory _containerFactory;
        public void ExecuteJob(IOddJob expr)
        {
            var instance = _containerFactory.CreateInstance(expr.TypeExecutedOn);
            var method = expr.TypeExecutedOn.GetMethod(expr.MethodName);
            method.Invoke(instance, expr.JobArgs);
        }
    }
}