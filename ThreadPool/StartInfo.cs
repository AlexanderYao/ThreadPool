using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    /// <summary>
    /// default：min = 2, max = 10, drop = never
    /// </summary>
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
