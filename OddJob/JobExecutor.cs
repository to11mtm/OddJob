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
            var method = expr.TypeExecutedOn.GetMethod(expr.MethodName);
            method.Invoke(instance, expr.JobArgs);
        }
    }
}