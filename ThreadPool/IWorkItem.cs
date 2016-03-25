using System;
using System.Threading;

namespace ThreadPool
{
    public interface IWorkItem
    {
        WaitCallback Callback { get; set; }
        string Name { get; set; }
        object State { get; set; }
    }
}
