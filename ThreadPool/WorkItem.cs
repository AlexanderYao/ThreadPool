using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    class WorkItem
    {
        internal String Name { get; set; }
        internal WaitCallback Callback { get; set; }
        internal Object State { get; set; }
    }
}
