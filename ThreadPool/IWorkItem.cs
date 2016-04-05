using System;
using System.Threading;

namespace ThreadPool
{
    public interface IWorkItem
    {
        WorkItemCallback Callback { get; }
        string Name { get; }
        object State { get; }
        //TODO: should not expose set, just use GetResult()
        IWorkResult Result { set; }

        IWorkResult GetResult();
    }
}
