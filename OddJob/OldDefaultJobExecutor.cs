using System.Linq;
using System.Reflection;
using GlutenFree.OddJob.Interfaces;

namespace GlutenFree.OddJob
{
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
            MethodInfo method = null;
            if (expr.MethodGenericTypes != null && expr.MethodGenericTypes.Length > 0)
            {
                method = expr.TypeExecutedOn.GetMethods().Where(q =>
                    q.IsGenericMethod && q.GetGenericArguments().Length == expr.MethodGenericTypes.Length).FirstOrDefault().MakeGenericMethod(expr.MethodGenericTypes);
            }
            else
            {
                method = expr.TypeExecutedOn.GetMethod(expr.MethodName);
            }
            
            method.Invoke(instance, expr.JobArgs.Select(q=>q.Value).ToArray());
            _containerFactory.Relase(instance);
        }
    }
}