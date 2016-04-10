using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;

namespace ThreadPool.Test
{
    [TestClass]
    public class TestShortTerm
    {
        private IThreadPool _pool;

        public TestShortTerm()
        {
            StartInfo info = new StartInfo { Timeout = 1, MinWorkerThreads = 1 };
            _pool = ThreadPoolFactory.Create(info, "short term pool");
        }

        [TestMethod]
        public void TestShortTerm_MinThreadNeverExit()
        {
            for (int i = 0; i < 3; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
                Thread.Sleep(1000);
            }

            Assert.AreEqual(_pool.ThreadCount, 1);
        }

        [TestMethod]
        public void TestShortTerm_QueueItemHasToWait()
        {
            for (int i = 0; i < 5; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            Assert.IsTrue(_pool.QueueCount > _pool.ThreadCount);

            //_pool.WaitAll();
            Thread.Sleep(3000);

            Assert.AreEqual(0, _pool.QueueCount);
            Assert.AreEqual(1, _pool.ThreadCount);
        }

        private Object Print(Object o)
        {
            Debug.WriteLine(o as String);
            return null;
        }
    }
}
