using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool.Test
{
    [TestClass]
    public class TestLongTerm
    {
        private IThreadPool _pool;

        public TestLongTerm()
        {
            StartInfo info = new StartInfo
            {
                Timeout = 5,
                MinWorkerThreads = 1,
                MaxWorkerThreads = 5,
            };
            _pool = ThreadPoolFactory.Create(info, "long term pool");
        }

        [TestMethod]
        public void TestLongTerm_Use4Threads()
        {
            for (int i = 0; i < 7; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            _pool.WaitForAll();
            Thread.Sleep(7000);
            Assert.IsTrue(_pool.MaxThreadCount == 4);
        }

        [TestMethod]
        public void TestLongTerm_Use5Threads()
        {
            for (int i = 0; i < 8; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            _pool.WaitForAll();
            Thread.Sleep(7000);
            Assert.IsTrue(_pool.MaxThreadCount == 5);
        }

        [TestMethod]
        public void TestLongTerm_20ItemUse5Threads()
        {
            for (int i = 0; i < 20; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            _pool.WaitForAll();
            Thread.Sleep(15000);
            Assert.IsTrue(_pool.MaxThreadCount == 5);
        }

        private Object Print(Object o)
        {
            Thread.Sleep(1000);
            Debug.WriteLine(o as String);
            return null;
        }
    }
}
