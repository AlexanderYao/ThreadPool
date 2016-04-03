using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Diagnostics;

namespace ThreadPool.Test
{
    [TestClass]
    public class TestAction
    {
        private IThreadPool _pool;

        public TestAction()
        {
            StartInfo info = new StartInfo { Timeout = 5 };
            _pool = new SingleThreadPool(info, "short term");
        }

        [TestMethod]
        public void TestTimeout_ExitByItself()
        {
            for (int i = 0; i < 5; i++)
            {
                _pool.QueueUserWorkItem(Print, "i'm item " + i);
                Thread.Sleep(1000);
            }

            Thread.Sleep(1000);
            Assert.AreEqual(_pool.ThreadCount, 1);

            Thread.Sleep(5000);
            Assert.AreEqual(_pool.ThreadCount, 0);
        }

        private void Print(Object o)
        {
            Console.WriteLine(o as String);
        }
    }
}
