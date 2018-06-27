using System;
using System.Linq;
using System.Threading;
using OddJob.Storage.TestKit;
using Xunit;

namespace OddJob.Storage.FileSystem.Test
{
    public class FileSystemStoreTests : StorageTests
    {
        public FileSystemStoreTests()
        {
        }

        protected override Func<IJobQueueAdder> jobAddStoreFunc { get{return ()=> new FileSystemJobQueueAdder("test.json"); } }
        protected override Func<IJobQueueManager> jobMgrStoreFunc { get {return ()=> new FileSystemJobQueueManager("test.json"); } }
    }
}
