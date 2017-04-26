using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SimulaDesign.ImportCore
{
    public class TraceLog
    {
        public static TraceSource GetLogger<T>(string methodName=null)
        {
            var name = typeof (T).FullName;
            if (!String.IsNullOrEmpty(methodName))
            {
                name += "." + methodName;
            }
            return GetLogger(name);
        }

        public static TraceSource GetLogger(string name)
        {
            var ts = new TraceSource(name, SourceLevels.Information);
            if (Trace.Listeners.Count > 0)
            {
                ts.Listeners.AddRange(Trace.Listeners);
            }
            else
            {
                var tempPath = Path.GetTempPath();
                var logFile = Path.Combine(tempPath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");
                ts.Listeners.Add(new CustomTextListener(logFile));
            }
            
            return ts;
        }
    }
}
