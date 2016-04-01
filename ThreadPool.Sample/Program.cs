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
            StartInfo info = new StartInfo { Timeout = 2 };
            IThreadPool pool = new SingleThreadPool(info, "short term");
            pool.QueueUserWorkItem(Print, "i'm item 1");
            pool.QueueUserWorkItem(Print, "i'm item 2");
            //pool.WaitForAll();
            Thread.Sleep(2000);
            Console.WriteLine(pool);
            Console.Read();
        }

        static void Print(Object o)
        {
            Console.WriteLine(o as String);
        }
    }
}
