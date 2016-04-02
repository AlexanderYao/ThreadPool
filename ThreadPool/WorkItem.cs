using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    class WorkItem : IWorkItem
    {
        public String Name { get; set; }
        public WaitCallback Callback { get; set; }
        public Object State { get; set; }
        public Object Result { get; set; }
    }
}
