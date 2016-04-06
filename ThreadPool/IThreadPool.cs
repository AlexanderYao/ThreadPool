using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public delegate object WorkItemCallback(object state);

    public interface IThreadPool
    {
        IWorkItem QueueUserWorkItem(WorkItemCallback callback, Object state, String name = "");

        bool WaitAll();

        bool WaitAll(int millisecondsTimeout);

        bool WaitAll(TimeSpan timeout);

        void Close();

        string Name { get; }

        StartInfo StartInfo { get; set; }

        int QueueCount { get; }

        int ThreadCount { get; }

        int MaxThreadCount { get; }
    }
}
