using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadPool
{
    internal static class Util
    {
        internal static string Format(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("message:{0}", ex.Message));
            sb.AppendLine(string.Format("StackTrace:{0}", ex.StackTrace));
            sb.AppendLine(string.Format("Source:{0}", ex.Source));

            if (null != ex.InnerException)
            {
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(Format(ex.InnerException));
            }

            return sb.ToString();
        }
    }
}
