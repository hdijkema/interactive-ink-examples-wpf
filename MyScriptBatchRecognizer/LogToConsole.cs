using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyScriptBatchRecognizer
{
    class LogToConsole : ILogMessage
    {
        bool log_debug = false;

        private String now()
        {
            DateTime dt = DateTime.Now;
            String tm = dt.Hour + ":" + dt.Minute + ":" + dt.Second + "." + dt.Millisecond;
            return tm;
        }

        public void logRemaining()
        {
            // does nothing
        }

        public void logDebug(string message)
        {
            if (log_debug) { Console.WriteLine(now() + " debug " + message); }
        }

        public void logError(string message)
        {
            Console.WriteLine(now() + " ERROR " + message);
        }

        public void logInfo(string message)
        {
            Console.WriteLine(now() + " info  " + message);
        }

        public void setLogDebug(bool yes)
        {
            log_debug = yes;
        }

        public bool debug()
        {
            return log_debug;
        }
    }
}
