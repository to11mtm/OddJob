using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;
using OddJob.Rpc;

namespace OddJob.RpcServer
{
    public interface IJobHubReceiver
    {
        void JobCreated(StreamingJobRequest jobData);
    }
}