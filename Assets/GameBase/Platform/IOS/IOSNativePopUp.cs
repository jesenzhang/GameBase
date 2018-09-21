using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBase
{
#if UNITY_IPHONE
    public class IOSNativePopUp
    {
        [DllImport("__Internal")]
        private static extern void _ShowMessage(string title, string message, string ok, int showID, int funcID);

        [DllImport("__Internal")]
        private static extern void _ShowDialog(string title, string message, string ok, string cancel, int showID, int funcID);

        [DllImport("__Internal")]
        private static extern void _DismissCurrentAlert();


        public static void ShowMessage(string title, string message, string ok, int showID, int funcID)
        {
            _ShowMessage(title, message, ok, showID, funcID);
        }

        public static void ShowDialog(string title, string message, string ok, string cancel, int showID, int funcID)
        {
            _ShowDialog(title, message, ok, cancel, showID, funcID);
        }

        public static void DismissCurrentAlert()
        {
            _DismissCurrentAlert();
        }
    }
#endif
}
