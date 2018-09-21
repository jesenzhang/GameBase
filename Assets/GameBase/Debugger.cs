using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static class Debugger
    {
        private static Action<object> log = null;
        private static Action<object> logWarning = null;
        private static Action<object> logError = null;
        private static bool dolog = true;


        public static void Init(Action<object> lg, Action<object> warning, Action<object> error)
        {
            log = lg;
            logWarning = warning;
            logError = error;
        }

        public static void SetPrintLog(bool v)
        {
            dolog = v;
        }

        public static void LogError(string format, params object[] objs) 
        {
            if (dolog && logError != null)
                logError(string.Format(format, objs));
        }

        public static void LogError_Fixed(string data)
        {
            if (dolog && logError != null)
                logError(data);
        }

        public static void Log(string format, params object[] objs)
        {
            if (dolog && log != null)
                log(string.Format(format, objs));
        }

        public static void Log_Fixed(string data)
        {
            if (dolog && log != null)
                log(data);
        }

        public static void LogWarning(string format, params object[] objs)
        {
            if (dolog && logWarning != null)
                logWarning(string.Format(format, objs));
        }

        public static void LogWarning_Fixed(string data)
        {
            if (dolog && logWarning != null)
                logWarning(data);
        }
    }
}
