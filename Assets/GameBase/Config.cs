using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static class Config
    {
        private static bool debugLog = true;
        private static bool detailDebugLog = false;

        private static bool directlyLoadResource = false;


        public static bool DirectlyLoadResource()
        {
            return directlyLoadResource;
        }

        public static void Set_DirectlyLoadResource(bool v)
        {
            directlyLoadResource = v;
        }

        public static bool Debug_Log()
        {
            return debugLog;
        }

        public static void Set_Debug_Log(bool v)
        {
            debugLog = v;
        }

        public static bool Detail_Debug_Log()
        {
            return detailDebugLog;
        }

        public static void Set_Detail_Debug_Log(bool v)
        {
            detailDebugLog = v;
        }

        public static void Set_Print_Log(bool v)
        {
            Debugger.SetPrintLog(v);
        }
    }
}
