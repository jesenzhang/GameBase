using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBase
{
    public static class AndroidNativePopUp
    {
        public static void ShowMessage(string title, string message, string ok, int showID, int funcID)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
            }
        }

        public static void ShowDialog(string title, string message, string ok, string cancel, int showID, int funcID)
        {
        }
    }
}
