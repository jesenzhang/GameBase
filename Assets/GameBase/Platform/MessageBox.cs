using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LuaInterface;


namespace GameBase
{
    public class MessageBox : MonoBehaviour
    {
        private static MessageBox backCom = null;
        private static bool inited = false;
        private static LuaFunction lua_OnMessage = null;
        private static LuaFunction lua_OnDialog = null;


        internal void OnMessageCallBack(string result)
        {
            if (lua_OnMessage == null)
                lua_OnMessage = LuaManager.GetFunction("MessageBox.OnMessage");
            if (lua_OnMessage != null)
                LuaManager.CallFunc_V(lua_OnMessage, result);
        }

        internal void OnDialogCallBack(string result)
        {
            if (lua_OnDialog == null)
                lua_OnDialog = LuaManager.GetFunction("MessageBox.OnDialog");
            if (lua_OnDialog != null)
                LuaManager.CallFunc_V(lua_OnDialog, result);
        }

        //static func
        public static void Init()
        {
            if (inited)
                return;
            inited = true;
            GameObject backGO = new GameObject();
            backGO.name = "MessageBox";
            Object.DontDestroyOnLoad(backGO);

            backCom = backGO.AddComponent<MessageBox>();
        }

        public static void MessageCallBack(string result)
        {
            if (backCom != null)
                backCom.OnMessageCallBack(result);
        }

        public static void DialogCallBack(string result)
        {
            if (backCom != null)
                backCom.OnDialogCallBack(result);
        }

        public static void ShowMessage(string title, string message, string ok, int showID, int funcID)
        {
#if UNITY_IPHONE
            IOSNativePopUp.ShowMessage(title, message, ok, showID, funcID);
#elif UNITY_ANDROID
            if(Application.platform == RuntimePlatform.Android)
                AndroidNativePopUp.ShowMessage(title, message, ok, showID, funcID);
            else
                WindowsNativePopUp.ShowMessage(title, message, ok, showID, funcID);
#endif
        }

        public static void ShowDialog(string title, string message, string ok, string cancel, int showID, int funcID)
        {
#if UNITY_IPHONE
            IOSNativePopUp.ShowDialog(title, message, ok, cancel, showID, funcID);
#elif UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
                AndroidNativePopUp.ShowDialog(title, message, ok, cancel, showID, funcID);
            else
                WindowsNativePopUp.ShowDialog(title, message, ok, cancel, showID, funcID);
#endif
        }
    }
}
