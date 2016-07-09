using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    internal class WorkThread : IThread
    {
        private int _timeout;
        private bool _isMin;
        private Thread _thread;
        private bool _isStop;
        private AutoResetEvent _event;
        private IWorkItem _item;
        private object _syncRoot;
        private string _name;

        public WorkThread(int timeout, bool isMin)
        {
            _timeout = timeout;
            _isMin = isMin;
            _isStop = false;
            _event = new AutoResetEvent(false);
            _syncRoot = new object();

            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.MTA);

            this.Id = _thread.ManagedThreadId;
        }

        public int Id
        {
            get;
            private set;
        }

        public string Name
        {
            get { return _name; }
            private set
            {
                lock (_thread)
                {
                    _name = value;
                    typeof(Thread).GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_thread, value);
                    var threadHandle = typeof(Thread).GetMethod("GetNativeHandle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_thread, null);
                    typeof(Thread).GetMethod("InformThreadNameChange", BindingFlags.NonPublic | BindingFlags.Static).Invoke(_thread, new object[] { threadHandle, value, (null == value) ? value.Length : 0 });
                }
            }
        }

        public DateTime StartTime
        {
            get;
            private set;
        }

        public IWorkItem WorkItem
        {
            get
            {
                lock (_syncRoot)
                {
                    return _item;
                }
            }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException();
                }

                lock (_syncRoot)
                {
                    _item = value;
                    Name = value.Name;
                    _event.Set();
                }
            }
        }

        public System.Threading.ThreadState State
        {
            get
            {
                return null == _thread ? System.Threading.ThreadState.Stopped : _thread.ThreadState;
            }
        }

        public bool IsStop { get { return _isStop; } }

        public bool IsMin { get { return _isMin; } }

        public event ItemFinishedHandler ItemFinished;

        public event ExitedHandler Exited;

        public void Start()
        {
            _thread.Start();
            StartTime = DateTime.Now;
        }

        public void Stop()
        {
            //do nothing, just wait and exit by itself
        }

        public void Dispose()
        {
            _thread = null;
            _event.Dispose();
            _event = null;
        }

        private void Loop()
        {
            while (!_isStop)
            {
                if (null == WorkItem)
                {
                    //may get last Set()
                    bool getIt = _event.WaitOne(_timeout * 1000);
                    Debug.WriteLine("thread {0}: {1}", Id, getIt ? "get it" : "not get it");

                    //min thread will never exit
                    _isStop = !getIt && !IsMin;

                    if (!getIt)
                    {
                        continue;
                    }
                }
                else
                {
                    Debug.WriteLine("thread {0}: already has it", Id);
                }

                IWorkResult result = new WorkResult();
                try
                {
                    //may get last Set(), and WorkItem has been done
                    if (null == WorkItem)
                    {
                        Debug.WriteLine("thread {0} [{1}]: item is null, continue", Id, IsMin ? "is min" : "is not min");
                        continue;
                    }

                    if (WorkItem.IsCancel)
                    {
                        result.Exception = new CancelException();
                        continue;
                    }

                    result.Result = WorkItem.Callback.Invoke(WorkItem.State);
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                    Debug.WriteLine(ex.Format());
                }
                finally
                {
                    if (null != WorkItem)
                    {
                        WorkItem.Result = result;
                        OnItemFinished();
                        Interlocked.Exchange(ref _item, null);
                    }
                }
            }

            OnExited();
        }

        private void OnExited()
        {
            if (null != Exited)
            {
                Exited(this, EventArgs.Empty);
            }
        }

        private void OnItemFinished()
        {
            if (null != ItemFinished)
            {
                ItemFinished(this, EventArgs.Empty);
            }
        }
    }
}
