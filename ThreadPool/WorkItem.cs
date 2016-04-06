using System;
using System.Threading;

namespace ThreadPool
{
    internal class WorkItem : IWorkItem
    {
        private ManualResetEvent _event;
        private IWorkResult _result;

        internal WorkItem()
        {
            _event = new ManualResetEvent(false);
        }

        public String Name { get; set; }
        public WorkItemCallback Callback { get; set; }
        public Object State { get; set; }
        public WaitHandle WaitHandle { get { return _event; } }

        public IWorkResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                _event.Set();
            }
        }

        public IWorkResult GetResult()
        {
            return this.GetResult(-1);
        }

        public IWorkResult GetResult(int millisecondsTimeout)
        {
            _event.WaitOne(millisecondsTimeout);
            return _result;
        }

        public IWorkResult GetResult(TimeSpan timeout)
        {
            return this.GetResult((int)timeout.TotalMilliseconds);
        }
    }
}
