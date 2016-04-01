using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    /// <summary>
    /// default：min = 2, max = 10, drop = never, timeout = 60s
    /// </summary>
    public class StartInfo
    {
        /// <summary>
        /// default: min = 2
        /// </summary>
        public Int32 MinWorkerThreads { get; set; }
        /// <summary>
        /// default: max = 10
        /// </summary>
        public Int32 MaxWorkerThreads { get; set; }
        /// <summary>
        /// default: drop = never
        /// </summary>
        public DropEnum DropEnum { get; set; }
        /// <summary>
        /// default: timeout = 60s, based on second
        /// </summary>
        public Int32 Timeout { get; set; }

        public StartInfo()
        {
            MinWorkerThreads = 2;
            MaxWorkerThreads = 10;
            DropEnum = DropEnum.DropNewest;
            Timeout = 60;
        }
    }

    public enum DropEnum
    {
        /// <summary>
        /// unlimited queue length, never drop
        /// </summary>
        Never = 0,
        /// <summary>
        /// when queue is full, drop the new coming
        /// </summary>
        DropNewest = 1,
        /// <summary>
        /// when queue is full, dequeue the head and drop it
        /// </summary>
        DropOldest = 2
    }
}
