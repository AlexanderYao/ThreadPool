using System;

namespace ThreadPool
{
    internal class WorkItem : IWorkItem
    {
        public String Name { get; set; }
        public WorkItemCallback Callback { get; set; }
        public Object State { get; set; }
        public Object Result { get; set; }
        public Exception Exception { get; set; }
    }
}
