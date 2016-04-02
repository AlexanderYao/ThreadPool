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
        private ConcurrentStack<IThread> _threads;
        private StartInfo _info;
        private ConcurrentQueue<IWorkItem> _queue;
        private Int32 _count;

        public SingleThreadPool() : this(null, string.Empty) { }

        public SingleThreadPool(StartInfo info, string name)
        {
            _info = info ?? new StartInfo();
            _queue = new ConcurrentQueue<IWorkItem>();
            _threads = new ConcurrentStack<IThread>();
            this.Name = name;

            for (int i = 0; i < _info.MinWorkerThreads; i++)
            {
                _threads.Push(NewThread());
            }
        }

        public string Name { get; private set; }

        public int QueueCount
        {
            get { return _queue.Count; }
        }

        public int ThreadCount
        {
            get { return _threads.Count; }
        }

        public bool IsStop { get; set; }

        public void QueueUserWorkItem(WaitCallback callback, Object state, String name = "")
        {
            if(IsStop)
            {
                return;
            }

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

            AddThread_IfNecessary();
            FindIdleThread_DoWork();
        }

        public void WaitForAll()
        {

        }

        public void Close()
        {
            IsStop = true;

            IThread t;
            bool hasThread = true;
            while (hasThread)
            {
                hasThread = _threads.TryPop(out t);

                if(hasThread)
                {
                    t.Stop();
                }
            }
            _threads.Clear();
            _threads = null;

            IWorkItem item;
            while (_queue.TryDequeue(out item))
            {
                //just drop it
            }
            _queue = null;
        }

        public override string ToString()
        {
            return string.Format("ThreadPool = {0}, QueueCount = {1}, ThreadCount = {2}", Name, QueueCount, ThreadCount);
        }

        private void AddThread_IfNecessary()
        {
            if (_queue.Count > _threads.Count && _threads.Count < _info.MaxWorkerThreads)
            {
                _threads.Push(NewThread());
            }
        }

        private IThread NewThread()
        {
            IThread _thread = new WorkThread(_info.Timeout);
            _thread.FinishItem += thread_FinishItem;
            _thread.Start();
            return _thread;
        }

        private void thread_FinishItem(object sender, ItemEventArgs e)
        {
            _threads.Push(sender as IThread);
        }

        private void FindIdleThread_DoWork()
        {
            IThread t;
            bool hasThread = _threads.TryPop(out t);

            if (!hasThread)
            {
                throw new Exception("find no idle thread");
            }

            IWorkItem item;
            bool hasItem = _queue.TryDequeue(out item);

            if (hasItem)
            {
                t.WorkItem = item;
            }
        }
    }
}
