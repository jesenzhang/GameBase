using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameBase
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PackItem
    {
        public System.Int16 x;
        public System.Int16 y;
        public System.Int16 w;
        public System.Int16 h;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct vector2usi
    {
        public System.UInt16 x;
        public System.UInt16 y;
    }
}
