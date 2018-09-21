using AOT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBase
{
    public static class DllHelper
    {
#if !UNITY_EDITOR && UNITY_IPHONE
        private const string SDLL = "__Internal";
#else
        private const string SDLL = "serioso";
#endif

       [DllImport(SDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void serioso_init(IntPtr debug_func, int msg_max);


#if !UNITY_EDITOR && UNITY_IPHONE
        private const string SNAVDLL = "__Internal";
#else
        private const string SNAVDLL = "serioso-nav";
#endif

        [DllImport(SNAVDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void SNavLua_Init(IntPtr L);

        [DllImport(SNAVDLL, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SNavAPI_InitLog(IntPtr debug_func);


        private delegate void Func_Arg_Str(string str);
        [MonoPInvokeCallback(typeof(Func_Arg_Str))]
        private static void Debug_Log(string str)
        {
            Debugger.Log(str);
        }

        public static void Init()
        {
            Func_Arg_Str func = Debug_Log;
            IntPtr fn = Marshal.GetFunctionPointerForDelegate(func);
            serioso_init(fn, 1024 * 1024 * 4);
            SNavAPI_InitLog(fn);
        }
    }
}
