using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public delegate void FinishItemHandler(object sender, ItemEventArgs e);
    public interface IThread
    {
        int Id { get; }
        string Name { get; }
        DateTime StartTime { get; }
        IWorkItem WorkItem { get; set; }
        ThreadState State { get; }

        event FinishItemHandler FinishItem;

        void Start();
        void Stop();
    }
    public class ItemEventArgs
    {
        public Object Result { get; set; }
    }
}
