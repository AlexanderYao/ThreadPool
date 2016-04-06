using System;
using System.Threading;

namespace ThreadPool
{
    public interface IWorkItem
    {
        WorkItemCallback Callback { get; }
        string Name { get; }
        object State { get; }
        WaitHandle WaitHandle { get; }
        IWorkResult Result { set; }

        IWorkResult GetResult();
        IWorkResult GetResult(int millisecondsTimeout);
        IWorkResult GetResult(TimeSpan timeout);
    }
}
