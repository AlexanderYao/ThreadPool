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
        //void QueueUserWorkItem(WaitCallback callback, Object state, String name = "");

        IWorkItem QueueUserWorkItem(WorkItemCallback callback, Object state, String name = "");

        void WaitForAll();

        void Close();

        string Name { get; }

        StartInfo StartInfo { get; set; }

        int QueueCount { get; }

        int ThreadCount { get; }

        int MaxThreadCount { get; }
    }
}
