using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    public interface IWorkResult
    {
        object Result { get; set; }
        Exception Exception { get; set; }
    }
}
