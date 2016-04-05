using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    internal class WorkResult : IWorkResult
    {
        public object Result
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }
    }
}
