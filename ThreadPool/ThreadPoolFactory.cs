using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    public class ThreadPoolFactory
    {
        public static IThreadPool Create(StartInfo info, string name)
        {
            return new SingleThreadPool(info, name);
        }
    }
}
