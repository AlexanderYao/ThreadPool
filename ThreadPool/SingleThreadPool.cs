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
    internal class SingleThreadPool : IThreadPool
    {
        private StartInfo _info;
        private Thread _mainThread;
        private ConcurrentStack<IThread> _threads;
        private ConcurrentQueue<IWorkItem> _queue;
        //TODO: should we use thread-safe list?
        private List<WaitHandle> _handles;
        private Int32 _count;
        private Int32 _threadCount;
        private Int32 _maxThreadCount;
        private bool _isStop;
        private AutoResetEvent _event;

        public SingleThreadPool() : this(null, string.Empty) { }

        public SingleThreadPool(StartInfo info, string name)
        {
            CheckStartInfo(info);
            _isStop = false;
            _info = info ?? new StartInfo();
            _queue = new ConcurrentQueue<IWorkItem>();
            _threads = new ConcurrentStack<IThread>();
            _handles = new List<WaitHandle>();
            _event = new AutoResetEvent(false);
            this.Name = name;

            for (int i = 0; i < _info.MinWorkerThreads; i++)
            {
                _threads.Push(NewThread(true));
            }

            _mainThread = new Thread(Loop);
            _mainThread.Name = string.IsNullOrEmpty(name) ? "Single Thread Pool" : name;
            _mainThread.IsBackground = true;
            _mainThread.Start();
        }

        public string Name { get; private set; }

        public StartInfo StartInfo
        {
            get { return _info; }
            set
            {
                CheckStartInfo(value);

                if (value.MinWorkerThreads != _info.MinWorkerThreads)
                {
                    throw new ArgumentException("Min can not change");
                }

                _info = value;
            }
        }

        private void CheckStartInfo(StartInfo value)
        {
            if (null == value)
            {
                throw new ArgumentNullException("StartInfo");
            }

            if (value.MinWorkerThreads <= 0 || value.MaxWorkerThreads <= 0 ||
                value.AdjustInterval <= 0 || value.MaxQueueCount <= 0 || value.Timeout <= 0)
            {
                throw new ArgumentException("Min|Max|Adjust|Queue|Timeout should <= 0");
            }

            if (value.MinWorkerThreads > value.MaxWorkerThreads)
            {
                throw new ArgumentException("Min > Max");
            }
        }

        public int QueueCount
        {
            get { return _queue.Count; }
        }

        public int ThreadCount
        {
            get { return _threadCount; }
        }

        public int MaxThreadCount
        {
            get { return _maxThreadCount; }
        }

        public IWorkItem QueueUserWorkItem(WorkItemCallback callback, Object state, String name = "")
        {
            if (_isStop)
            {
                return null;
            }

            IWorkItem item = new WorkItem
            {
                Callback = callback,
                State = state,
                Name = string.IsNullOrEmpty(name) ? ("item " + _count) : name,
            };
            _count++;

            EnqueueOrDrop(item);
            return item;
        }

        public bool WaitAll()
        {
            return WaitHandle.WaitAll(_handles.ToArray());
        }

        public bool WaitAll(int millisecondsTimeout)
        {
            return WaitHandle.WaitAll(_handles.ToArray(), millisecondsTimeout);
        }

        public bool WaitAll(TimeSpan timeout)
        {
            return WaitHandle.WaitAll(_handles.ToArray(), timeout);
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

        private void EnqueueOrDrop(IWorkItem item)
        {
            if (_queue.Count < _info.MaxQueueCount)
            {
                _queue.Enqueue(item);
                _handles.Add(item.WaitHandle);
                Debug.WriteLine("{0} enqueue", new object[] { item.Name });
                _event.Set();
            }
            else
            {
                switch (_info.DropEnum)
                {
                    case DropEnum.Never:
                        _queue.Enqueue(item);
                        _handles.Add(item.WaitHandle);
                        Debug.WriteLine("{0} enqueue", new object[] { item.Name });
                        _event.Set();
                        break;
                    case DropEnum.DropNewest:
                        //do nothing, just drop it
                        item.Result = new WorkResult { Exception = new CancelException() };
                        Debug.WriteLine("{0} dropped", new object[] { item.Name });
                        break;
                    case DropEnum.DropOldest:
                        IWorkItem dropItem;
                        bool removeFirst = _queue.TryDequeue(out dropItem);
                        //Assert.IsTrue(removeFirst);
                        _handles.Remove(dropItem.WaitHandle);
                        dropItem.Result = new WorkResult { Exception = new CancelException() };
                        Debug.WriteLine("{0} dropped", new object[] { dropItem.Name });

                        _queue.Enqueue(item);
                        _handles.Add(item.WaitHandle);
                        Debug.WriteLine("{0} enqueue", new object[] { item.Name });
                        _event.Set();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("DropEnum");
                }
            }
        }

        private IThread NewThread(bool isMin = false)
        {
            IThread _thread = new WorkThread(_info.Timeout, isMin);
            _thread.ItemFinished += thread_FinishItem;
            _thread.Exited += thread_Exited;
            _thread.Start();
            _threadCount++;
            _maxThreadCount = Math.Max(_maxThreadCount, _threadCount);
            return _thread;
        }

        private void thread_Exited(object sender, EventArgs e)
        {
            _threadCount--;

            IThread t = sender as IThread;
            if (null != t.WorkItem)
            {
                Debug.WriteLine("push {0} back into queue", new object[] { t.WorkItem.Name });
                _queue.Enqueue(t.WorkItem);
            }

            Debug.WriteLine("thread {0} has exited", t.Id);
        }

        private void thread_FinishItem(object sender, EventArgs e)
        {
            _threads.Push(sender as IThread);
        }

        private void Loop()
        {
            while (true)
            {
                //Debug.WriteLine(this);
                //adjust pool every 1 sec
                _event.WaitOne(1000 * _info.AdjustInterval);

                if (_queue.Count == 0)
                {
                    continue;
                }

                List<IThread> threads = TryGetThread();

                if (null == threads || threads.Count == 0)
                {
                    continue;
                }

                int i = 0;
                for (; i < threads.Count; i++)
                {
                    IWorkItem workItem;
                    bool hasItem = false;

                    do
                    {
                        hasItem = _queue.TryPeek(out workItem);

                        if (hasItem && workItem.IsCancel)
                        {
                            bool dropCancel = _queue.TryDequeue(out workItem);
                            //Assert.IsTrue(dropCancel);
                            _handles.Remove(workItem.WaitHandle);
                            workItem.Result = new WorkResult { Exception = new CancelException() };
                        }
                        else
                        {
                            break;
                        }
                    } while (true);

                    if (!hasItem)
                    {
                        //we break even if following thread can get item, to keep stack order
                        break;
                    }

                    IThread t = threads[i];
                    if (t.IsStop)
                    {
                        t.Dispose();
                        continue;
                    }
                    else
                    {
                        _queue.TryDequeue(out workItem);
                        Debug.WriteLine("assign {0} to thread {1}", workItem.Name, threads[i].Id);
                        threads[i].WorkItem = workItem;
                    }
                }

                //push from back end, keep the stack order
                for (int j = threads.Count - 1; j >= i; j--)
                {
                    _threads.Push(threads[j]);
                }
            }
        }

        private List<IThread> TryGetThread()
        {
            List<IThread> result = new List<IThread>();
            IThread t = null;

            while (true)
            {
                bool hasThread = _threads.TryPop(out t);

                if (!hasThread)
                {
                    if (_threadCount < _info.MaxWorkerThreads &&
                        result.Count < _queue.Count)
                    {
                        result.Add(NewThread());
                    }
                    break;
                }
                else// if (hasThread)
                {
                    if (t.IsStop)
                    {
                        t.Dispose();
                        continue;
                    }
                    else
                    {
                        result.Add(t);
                    }
                }
            }
            return result;
        }
    }
}
