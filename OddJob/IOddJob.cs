using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OddJob
{


    public interface IOddJob
    {
        Guid JobId { get; }
        object[] JobArgs { get; }
        Type TypeExecutedOn { get; }
        string MethodName { get; }
    }
}