using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public interface IThreadPool
    {
        void QueueUserWorkItem(WaitCallback callback, Object state, String name = "");

        void WaitForAll();
    }
}
