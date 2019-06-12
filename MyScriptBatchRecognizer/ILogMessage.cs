using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyScriptBatchRecognizer
{
    public interface ILogMessage
    {
        void setLogDebug(bool yes);
        bool debug();
        void logInfo(String message);
        void logError(String message);
        void logDebug(String message);
        void logRemaining();
    }
}
