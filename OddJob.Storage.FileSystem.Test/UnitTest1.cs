﻿using System;
using System.Linq;
using System.Threading;
using GlutenFree.OddJob.Interfaces;
using GlutenFree.OddJob.Storage.BaseTests;
using GlutenFree.OddJob.Storage.FileSystem;
using Xunit;

namespace OddJob.Storage.FileSystem.Test
{
    public class FileSystemStoreTests : StorageTests
    {
        public FileSystemStoreTests()
        {
        }

        protected override Func<IJobQueueAdder> JobAddStoreFunc { get{return ()=> new FileSystemJobQueueAdder("test.json"); } }
        protected override Func<IJobQueueManager> JobMgrStoreFunc { get {return ()=> new FileSystemJobQueueManager("test.json"); } }
    }
}
