using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public interface IThread : IDisposable
    {
        int Id { get; }
        string Name { get; }
        DateTime StartTime { get; }
        IWorkItem WorkItem { get; set; }
        ThreadState State { get; }
        bool IsStop { get; }
        //是否线程池必须运行的最小线程
        bool IsMin { get; set; }

        event ItemFinishedHandler ItemFinished;
        event ExitedHandler Exited;

        void Start();
        void Stop();
    }

    public delegate void ItemFinishedHandler(object sender, ItemEventArgs e);
    
    public delegate void ExitedHandler(object sender, EventArgs e);

    public class ItemEventArgs
    {
        public Object Result { get; set; }
    }
}
