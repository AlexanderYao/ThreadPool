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
            StartInfo info = new StartInfo { Timeout = 60 };
            IThreadPool pool = new SingleThreadPool(info, "short term");
            for (int i = 0; i < 20; i++)
            {
                pool.QueueUserWorkItem(Print, "i'm item " + i);
                Thread.Sleep(500);
                Console.WriteLine(pool);
            }
            //pool.WaitForAll();
            Console.Read();
        }

        static void Print(Object o)
        {
            Console.WriteLine(o as String);
        }
    }
}
