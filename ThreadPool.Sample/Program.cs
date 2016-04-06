using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadPool;

namespace ThreadPool.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestMyThreadPool();
            Console.Read();
        }

        private static void TestMyThreadPool()
        {
            StartInfo info = new StartInfo { Timeout = 1, MinWorkerThreads = 1 };
            IThreadPool pool = ThreadPoolFactory.Create(info, "long term pool");
            for (int i = 0; i < 5; i++)
            {
                pool.QueueUserWorkItem(Print, "i'm item " + i, "test");
            }
            pool.WaitAll();
        }

        static Object Print(Object o)
        {
            //Thread.Sleep(1000);
            Debug.WriteLine(o as String);
            return null;
        }

        private static void TestWaitHandle()
        {
            AutoResetEvent e = new AutoResetEvent(false);
            Thread t = new Thread(() =>
            {
                Debug.WriteLine("waiting for event...");
                bool getIt;
                for (int i = 0; i < 10; i++)
                {
                    getIt = e.WaitOne(1000);
                    Debug.WriteLine("get it? {0}", getIt ? "yes" : "no");
                }
            });
            //连续触发的话，只有1个Set有用
            e.Set();
            e.Set();
            Debug.WriteLine("event set!");
            t.Start();
        }
    }
}
