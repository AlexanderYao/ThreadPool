using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    public class StartInfo
    {
        public Int32 MinWorkerThreads { get; set; }
        public Int32 MaxWorkerThreads { get; set; }
        public DropEnum DropEnum { get; set; }

        public StartInfo()
        {
            MinWorkerThreads = 2;
            MaxWorkerThreads = 10;
            DropEnum = DropEnum.DropNewest;
        }
    }

    public enum DropEnum
    {
        /// <summary>
        /// when queue is full, drop the new coming
        /// </summary>
        DropNewest,
        /// <summary>
        /// when queue is full, dequeue the head and drop it
        /// </summary>
        DropOldest
    }
}
