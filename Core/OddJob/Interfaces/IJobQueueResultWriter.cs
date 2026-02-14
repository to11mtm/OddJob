using System;
using System.Threading.Tasks;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IJobQueueResultWriter
    {
        Task WriteJobQueueResult(Guid jobGuid, IOddJobResult result);
    }
}