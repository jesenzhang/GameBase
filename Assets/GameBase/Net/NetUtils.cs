using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static class NetUtils
    {
        public const byte MSG_HEADER_LEN = 7;
        public const byte MSG_HEADER_T = 1;
        private static byte _SCRIPTTOP_GROUP = 120;
        public static byte SCRIPTTOP_GROUP
        {
            get { return _SCRIPTTOP_GROUP; }
        }

        internal static void SetScriptTopGroup(int g)
        {
            _SCRIPTTOP_GROUP = (byte)g;
        }
    }
}
