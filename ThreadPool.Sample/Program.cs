using System;
using System.Collections.Generic;
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
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            //TestWaitHandle();
            TestThreadPool();
            Console.Read();
        }

        static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }

        private static void TestThreadPool()
        {
            StartInfo info = new StartInfo { Timeout = 1, MinWorkerThreads = 1 };
            IThreadPool pool = new SingleThreadPool(info, "long term pool");
            for (int i = 0; i < 5; i++)
            {
                pool.QueueUserWorkItem(Print, "i'm item " + i);
            }
            pool.WaitForAll();
        }

        static void Print(Object o)
        {
            //Thread.Sleep(1000);
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
            //连续触发的话，只有1个Set有用
            e.Set();
            e.Set();
            Console.WriteLine("event set!");
            t.Start();
        }
    }
}
