using System;
using System.Collections.Generic;
using System.Linq;
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
            //TestWaitHandle();
            TestThreadPool();
            Console.Read();
        }

        private static void TestThreadPool()
        {
            StartInfo info = new StartInfo
            {
                Timeout = 5,
                MinWorkerThreads = 1,
                MaxWorkerThreads = 5,
            };
            IThreadPool pool = new SingleThreadPool(info, "long term pool");
            for (int i = 0; i < 10; i++)
            {
                pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            pool.WaitForAll();
        }

        static void Print(Object o)
        {
            Thread.Sleep(1000);
            Console.WriteLine(o as String);
        }

        private static void TestWaitHandle()
        {
            AutoResetEvent e = new AutoResetEvent(false);
            Thread t = new Thread(() =>
            {
                Console.WriteLine("waiting for event...");
                bool getIt;
                for (int i = 0; i < 10; i++)
                {
                    getIt = e.WaitOne(1000);
                    Console.WriteLine("get it? {0}", getIt ? "yes" : "no");
                }
            });
            e.Set();
            Console.WriteLine("event set!");
            t.Start();
        }
    }
}
