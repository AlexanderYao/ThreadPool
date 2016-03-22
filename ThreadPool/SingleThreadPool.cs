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
        //private List<Thread> _threads;
        private Thread _thread;
        private AutoResetEvent _event;
        private ConcurrentQueue<WorkItem> _queue;
        private Int32 _count;

        public SingleThreadPool()
        {
            _event = new AutoResetEvent(false);
            _queue = new ConcurrentQueue<WorkItem>();

            _thread = new Thread(Loop);
            //_thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.MTA);
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
        }

        public void WaitForAll()
        {

        }

        public void Stop()
        {
            IsStop = true;
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

                WorkItem item = null;
                bool hasItem = _queue.TryDequeue(out item);

                if (!hasItem)
                {
                    continue;
                }

                SetThreadName(item.Name);
                item.Callback.Invoke(item.State);
            }
        }

        private void SetThreadName(String name)
        {
            Thread t = Thread.CurrentThread;

            lock (t)
            {
                t.GetType().GetField("m_Name", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(t, name);
                var threadHandle = t.GetType().GetMethod("GetNativeHandle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(t, null);
                t.GetType().GetMethod("InformThreadNameChange", BindingFlags.NonPublic | BindingFlags.Static).Invoke(t, new object[] { threadHandle, name, (null == name) ? name.Length : 0 });
            }
        }
    }
}
