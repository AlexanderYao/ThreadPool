using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadPool
{
    public class WorkThread : IThread
    {
        private const int TIME_OUT = 10 * 1000;
        private int _waitCount;
        private Thread _thread;
        private bool _isStop;
        private AutoResetEvent _event;
        private IWorkItem _item;
        private object _syncRoot;

        public WorkThread()
        {
            _waitCount = 0;
            _isStop = false;
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

        public ThreadState State { get { return _thread.ThreadState; } }

        public int WaitCount { get { return _waitCount; } }

        public bool IsIdle { get; private set; }

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
            while (!_isStop)
            {
                if (null == WorkItem)
                {
                    IsIdle = true;
                    bool getIt = _event.WaitOne(TIME_OUT);

                    if (!getIt)
                    {
                        _waitCount++;
                        continue;
                    }
                }

                lock (_syncRoot)
                {
                    IsIdle = false;
                    WorkItem.Callback.Invoke(WorkItem.State);
                    _item = null;
                    IsIdle = true;
                }
            }
        }
    }
}
