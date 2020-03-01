using System.Threading.Tasks;
using WebSharper;

namespace GlutenFree.OddJob.Manager.Presentation.WS_AspnetCore
{
    //Unfortunately, We need this for now.
    //Websharper doesn't appear to let you use interface methods marked with [Remote],
    //Only Abstract or Concrete classes.
    //But at the same time, we need to Decorate anything going through Websharper in the name of potential thread-safety.
    public class OddJobRemotingWrapper : IRemotingHandler<JobUpdateViewModel, bool>, IRemotingHandler<QueueNameListRequest, string[]>,
        IRemotingHandler<JobSearchCriteria, JobMetadataResult[]>, IRemotingHandler<GetMethodsForQueueNameRequest, string[]>
    {
        private IRemotingHandler<GetMethodsForQueueNameRequest, string[]> _methodHandlerImpl;
        private IRemotingHandler<JobSearchCriteria, JobMetadataResult[]> _searchHandlerImpl;
        private IRemotingHandler<JobUpdateViewModel, bool> _updateHandlerImpl;
        private IRemotingHandler<QueueNameListRequest, string[]> _queueNameHandlerImpl;

        public OddJobRemotingWrapper(IRemotingHandler<GetMethodsForQueueNameRequest, string[]> methodHandlerImpl, IRemotingHandler<JobSearchCriteria, JobMetadataResult[]> searchHandlerImpl, IRemotingHandler<JobUpdateViewModel, bool> updateHandlerImpl, IRemotingHandler<QueueNameListRequest, string[]> queueNameHandlerImpl)
        {
            _methodHandlerImpl = methodHandlerImpl;
            _searchHandlerImpl = searchHandlerImpl;
            _updateHandlerImpl = updateHandlerImpl;
            _queueNameHandlerImpl = queueNameHandlerImpl;
        }
        [Remote]
        public Task<string[]> Handle(GetMethodsForQueueNameRequest command)
        {
            return _methodHandlerImpl.Handle(command);
        }
        [Remote]
        public Task<JobMetadataResult[]> Handle(JobSearchCriteria command)
        {
            return _searchHandlerImpl.Handle(command);
        }
        [Remote]
        public Task<bool> Handle(JobUpdateViewModel command)
        {
            return _updateHandlerImpl.Handle(command);
        }
        [Remote]
        public Task<string[]> Handle(QueueNameListRequest command)
        {
            return _queueNameHandlerImpl.Handle(command);
        }
    }
}