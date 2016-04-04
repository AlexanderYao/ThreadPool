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
        private AutoResetEvent _event;
        private IWorkItem _item;
        private object _syncRoot;
        private string _name;

        public WorkThread(int timeout)
        {
            _timeout = timeout;
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
            get { return _item; }
            set
            {
                if (null == value)
                {
                    throw new ArgumentNullException();
                }

                _item = value;
                Name = value.Name;
                _event.Set();
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

        public bool IsMin { get; set; }

        public event ItemFinishedHandler ItemFinished;

        public event ExitedHandler Exited;

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
                if (null == _item)
                {
                    bool getIt = _event.WaitOne(_timeout * 1000);

                    //min thread will never exit
                    if (!getIt && !IsMin)
                    {
                        _isStop = true;
                        continue;
                    }
                }

                try
                {
                    //in debug mode, _event can be set while _item is still null, why?
                    if (null == _item)
                    {
                        continue;
                    }

                    _item.Callback.Invoke(_item.State);
                    OnItemFinished(_item.Result);
                    _item = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Format());
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

        private void OnItemFinished(object result)
        {
            if (null != ItemFinished)
            {
                ItemFinished(this, new ItemEventArgs { Result = result });
            }
        }
    }
}
