
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameBase
{
#if __WINDOWS__
    class WindowsNativePopUp
    {
        [DllImport("User32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        private static extern int MessageBox(IntPtr handle, String message, String title, int type);


        public static void ShowMessage(string title, string message, string ok, int showID, int funcID)
        {
            int re = MessageBox(IntPtr.Zero, message, title, 0);
            GameBase.MessageBox.MessageCallBack(showID + "^" + re + "^" + funcID);
        }

        public static void ShowDialog(string title, string message, string ok, string cancel, int showID, int funcID)
        {
            int re = MessageBox(IntPtr.Zero, message, title, 1);
            GameBase.MessageBox.DialogCallBack(showID + "^" + re + "^" + funcID);
        }
    }
#endif
}
