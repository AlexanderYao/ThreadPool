using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace ThreadPool.Test
{
    [TestClass]
    public class TestQueueStrategy
    {
        private StartInfo _info;
        private IThreadPool _pool;

        public TestQueueStrategy()
        {
            _info = new StartInfo
            {
                Timeout = 1,
                MinWorkerThreads = 1,
                MaxWorkerThreads = 1,
                MaxQueueCount = 3
            };
            _pool = ThreadPoolFactory.Create(_info, "pool - limited queue");
        }

        [TestMethod]
        public void TestQueueStrategy_Never()
        {
            for (int i = 0; i < 5; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }

            Assert.IsTrue(_pool.QueueCount > _info.MaxQueueCount);
        }

        [TestMethod]
        public void TestQueueStrategy_DropNewest()
        {
            _info.DropEnum = DropEnum.DropNewest;

            for (int i = 0; i < 5; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }

            Debug.WriteLine("{0} <= {1}", _pool.QueueCount, _info.MaxQueueCount);
            Assert.IsTrue(_pool.QueueCount <= _info.MaxQueueCount);
        }

        [TestMethod]
        public void TestQueueStrategy_DropOldest()
        {
            _info.DropEnum = DropEnum.DropOldest;

            for (int i = 0; i < 5; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }

            Debug.WriteLine("{0} <= {1}", _pool.QueueCount, _info.MaxQueueCount);
            Assert.IsTrue(_pool.QueueCount <= _info.MaxQueueCount);
        }

        private Object Print(Object o)
        {
            Debug.WriteLine(o as String);
            return null;
        }
    }
}
