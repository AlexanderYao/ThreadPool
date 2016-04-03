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
        private Thread _mainThread;
        private ConcurrentStack<IThread> _threads;
        private StartInfo _info;
        private ConcurrentQueue<IWorkItem> _queue;
        private Int32 _count;
        private Int32 _threadCount;
        private bool _isStop;
        private AutoResetEvent _event;

        public SingleThreadPool() : this(null, string.Empty) { }

        public SingleThreadPool(StartInfo info, string name)
        {
            _isStop = false;
            _info = info ?? new StartInfo();
            _queue = new ConcurrentQueue<IWorkItem>();
            _threads = new ConcurrentStack<IThread>();
            _event = new AutoResetEvent(false);
            this.Name = name;

            for (int i = 0; i < _info.MinWorkerThreads; i++)
            {
                _threads.Push(NewThread());
            }

            _mainThread = new Thread(Loop);
            _mainThread.Name = string.IsNullOrEmpty(name) ? "Single Thread Pool" : name;
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        public string Name { get; private set; }

        public int QueueCount
        {
            get { return _queue.Count; }
        }

        public int ThreadCount
        {
            get { return _threadCount; }
            set { _threadCount = value; }
        }

        public void QueueUserWorkItem(WaitCallback callback, Object state, String name = "")
        {
            if (_isStop)
            {
                return;
            }

            var item = new WorkItem
            {
                Callback = callback,
                State = state,
                Name = string.IsNullOrEmpty(name) ? ("item " + _count) : name,
            };
            _count++;
            _queue.Enqueue(item);
            //_event.Set();
        }

        public void WaitForAll()
        {

        }

        public void Close()
        {
            _isStop = true;

            IThread t;
            bool hasThread = true;
            while (hasThread)
            {
                hasThread = _threads.TryPop(out t);

                if (hasThread)
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

        private IThread NewThread()
        {
            IThread _thread = new WorkThread(_info.Timeout);
            _thread.ItemFinished += thread_FinishItem;
            _thread.Exited += thread_Exited;
            _thread.Start();
            _threadCount++;
            return _thread;
        }

        private void thread_Exited(object sender, EventArgs e)
        {
            _threadCount--;
        }

        private void thread_FinishItem(object sender, ItemEventArgs e)
        {
            _threads.Push(sender as IThread);
        }

        private void Loop()
        {
            while (true)
            {
                Console.WriteLine(this);
                //adjust pool every 1 sec
                _event.WaitOne(1000 * _info.AdjustInterval);

                if (_queue.Count == 0)
                {
                    continue;
                }

                IThread t = TryGetThread();

                if (null == t)
                {
                    continue;
                }

                IWorkItem workItem;
                bool hasItem = _queue.TryDequeue(out workItem);

                if (!hasItem)
                {
                    continue;
                }

                Console.WriteLine("assign {0} to thread {1}", workItem.Name, t.Id);
                t.WorkItem = workItem;
            }
        }

        private IThread TryGetThread()
        {
            IThread t = null;

            while (true)
            {
                bool hasThread = _threads.TryPop(out t);

                if (!hasThread)
                {
                    if (_threadCount < _info.MaxWorkerThreads)
                    {
                        t = NewThread();
                    }
                    break;
                }
                else if (hasThread)
                {
                    if (t.IsStop)
                    {
                        t.Dispose();
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return t;
        }
    }
}
