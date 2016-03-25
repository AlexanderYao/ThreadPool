using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public class SingleThreadPool : IThreadPool
    {
        private List<IThread> _threads;
        private StartInfo _info;
        private Thread _thread;
        private AutoResetEvent _event;
        private ConcurrentQueue<WorkItem> _queue;
        private Int32 _count;

        public SingleThreadPool() : this(null) { }

        public SingleThreadPool(StartInfo info)
        {
            _info = info ?? new StartInfo();
            _event = new AutoResetEvent(false);
            _queue = new ConcurrentQueue<WorkItem>();
            _threads = new List<IThread>(_info.MinWorkerThreads);

            for (int i = 0; i < _info.MinWorkerThreads; i++)
            {
                _threads.Add(NewThread());
            }

            _thread = new Thread(Loop);
            _thread.Start();
        }

        public bool IsStop { get; set; }

        public void QueueUserWorkItem(WaitCallback callback, Object state, String name = "")
        {
            var item = new WorkItem { Callback = callback, State = state };
            if (string.IsNullOrEmpty(name))
            {
                item.Name = string.Format("item {0}", _count);
            }
            else
            {
                item.Name = name;
            }

            _count++;
            _queue.Enqueue(item);
            _event.Set();
            Debug.Print("set event");

            Adjust();
        }

        public void WaitForAll()
        {

        }

        public void Stop()
        {
            IsStop = true;
        }

        private void Adjust()
        {
            if(_queue.Count > _threads.Count)
            {
                _threads.Add(NewThread());
            }

            foreach (var t in _threads)
            {
                if(t.IsIdle && t.WaitCount >= 6)
                {
                    t.Stop();
                }
            }
        }

        private IThread NewThread()
        {
            IThread _thread = new WorkThread();
            _thread.Start();
            return _thread;
        }

        private void Loop()
        {
            while (!IsStop)
            {
                if (_queue.Count == 0)
                {
                    Debug.Print("wait for event...");
                    _event.WaitOne();
                }

                IThread t = _threads.FirstOrDefault(e => e.IsIdle);
                if(null == t)
                {
                    //wait some time for idle thread
                    continue;
                }

                WorkItem item = null;
                bool hasItem = _queue.TryDequeue(out item);

                if (!hasItem)
                {
                    continue;
                }

                t.WorkItem = item;
            }
        }
    }
}
