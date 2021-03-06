﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class TestWaitForResult
    {
        private IThreadPool _pool;

        public TestWaitForResult()
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
        public void Test_WaitSingle()
        {
            List<int> list = new List<int>();
            for (int i = 0; i <= 10000; i++)
            {
                list.Add(i);
            }
            IWorkItem item = _pool.QueueUserWorkItem(Avg, list);
            Assert.AreEqual(5000, item.GetResult().Result);
        }

        [TestMethod]
        public void Test_WaitAll()
        {
            bool getAll = false;
            Thread t = new Thread(() =>
            {
                List<int> list = new List<int>();
                for (int i = 0; i <= 10000; i++)
                {
                    list.Add(i);
                }

                for (int i = 0; i < 10; i++)
                {
                    _pool.QueueUserWorkItem(Avg, list);
                }

                getAll = _pool.WaitAll();
            });
            t.SetApartmentState(ApartmentState.MTA);
            t.Start();
            t.Join();
            Assert.IsTrue(getAll);
        }

        private Object Avg(Object o)
        {
            List<int> list = o as List<int>;
            if (null == list || list.Count == 0)
            {
                return 0;
            }

            long sum = 0;
            foreach (var i in list)
            {
                sum += i;
            }
            return (int)(sum / (long)list.Count);
        }
    }
}
