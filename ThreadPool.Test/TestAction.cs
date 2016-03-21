using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ThreadPool.Test
{
    [TestClass]
    public class TestAction
    {
        private static IThreadPool _pool;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _pool = new SingleThreadPool();
        }

        [ClassCleanup]
        public static void End()
        {
            //_pool.Shutdown();
        }

        [TestMethod]
        public void Action0()
        {
            _pool.QueueUserWorkItem(Print, "hello world!");
        }

        private void Print(Object o)
        {
            Console.WriteLine(o as String);
        }
    }
}
