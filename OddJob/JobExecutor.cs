namespace OddJob
{
    public class JobExecutor
    {
        public IContainerFactory containerFactory = new DefaultContainerFactory();
        public void ExecuteJob(IOddJob expr)
        {
            var instance = containerFactory.CreateInstance(expr.TypeExecutedOn);
            var method = expr.TypeExecutedOn.GetMethod(expr.MethodName);
            method.Invoke(instance, expr.JobArgs);
        }
    }
}