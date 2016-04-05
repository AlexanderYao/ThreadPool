using System;
using System.Threading;

namespace ThreadPool
{
    public interface IWorkItem
    {
        WorkItemCallback Callback { get; }
        string Name { get; }
        object State { get; }
        object Result { get; set; }
        Exception Exception { get; set; }
    }
}
