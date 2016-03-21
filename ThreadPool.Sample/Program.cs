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
            IThreadPool pool = new SingleThreadPool();
            pool.QueueUserWorkItem(Print, "i'm item 1");
            pool.QueueUserWorkItem(Print, "i'm item 2");
            pool.WaitForAll();
        }

        static void Print(Object o)
        {
            Console.WriteLine(o as String);
        }
    }
}
