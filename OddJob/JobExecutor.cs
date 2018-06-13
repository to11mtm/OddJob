namespace OddJob
{
    public class JobExecutor
    {
        public JobExecutor(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;
        }
        private IContainerFactory _containerFactory = new DefaultContainerFactory();
        public void ExecuteJob(IOddJob expr)
        {
            var instance = _containerFactory.CreateInstance(expr.TypeExecutedOn);
            var method = expr.TypeExecutedOn.GetMethod(expr.MethodName);
            method.Invoke(instance, expr.JobArgs);
        }
    }
}