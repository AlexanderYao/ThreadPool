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
    public class WorkThread : IThread
    {
        private int _timeout;
        private Thread _thread;
        private bool _isStop;
        private bool _isIdle;
        private AutoResetEvent _event;
        private IWorkItem _item;
        private object _syncRoot;

        public WorkThread(int timeout)
        {
            _timeout = timeout;
            _isStop = false;
            _isIdle = false;
            _event = new AutoResetEvent(false);
            _syncRoot = new object();

            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.MTA);
        }

        public int Id
        {
            get { return _thread.ManagedThreadId; }
        }

        public string Name
        {
            get { return _thread.Name; }
            private set
            {
                lock (_thread)
                {
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
            get { return _item; }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException();
                }

                lock (_syncRoot)
                {
                    _item = value;
                }
                Name = value.Name;
                _event.Set();
            }
        }

        public System.Threading.ThreadState State { get { return _thread.ThreadState; } }

        public event FinishItemHandler FinishItem;

        public void Start()
        {
            _thread.Start();
            StartTime = DateTime.Now;
        }

        public void Stop()
        {
            _isStop = true;
            _event.Set();
        }

        private void Loop()
        {
            while (!_isStop && !_isIdle)
            {
                if (null == WorkItem)
                {
                    bool getIt = _event.WaitOne(_timeout * 1000);

                    if (!getIt)
                    {
                        _isIdle = true;
                        continue;
                    }
                }

                lock (_syncRoot)
                {
                    try
                    {
                        WorkItem.Callback.Invoke(WorkItem.State);
                        OnFinishItem(WorkItem.Result);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Format());
                    }
                    _item = null;
                }
            }
        }

        private void OnFinishItem(object result)
        {
            if (null != FinishItem)
            {
                FinishItem(this, new ItemEventArgs { Result = result });
            }
        }
    }
}
