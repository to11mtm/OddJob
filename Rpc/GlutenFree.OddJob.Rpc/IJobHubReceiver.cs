using System.Threading.Tasks;
using GlutenFree.OddJob.Serializable;

namespace OddJob.RpcServer
{
    public interface IJobHubReceiver
    {
        void JobCreated(SerializableOddJob jobData);
    }
}