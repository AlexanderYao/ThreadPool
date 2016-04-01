using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public interface IThread
    {
        int Id { get; }
        string Name { get; }
        DateTime StartTime { get; }
        IWorkItem WorkItem { get; set; }
        ThreadState State { get; }
        void Start();
        void Stop();
    }
}
