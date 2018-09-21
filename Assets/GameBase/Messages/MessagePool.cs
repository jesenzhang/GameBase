#define LUASCRIPT
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

namespace GameBase
{
    public class MessagePool
    {
        public const byte OnNetMessageArrived = 1;
        public const byte OnApplicationUpdate = 2;
        public const byte OnApplicationFixupdate = 3;
        public const byte OnApplicationLastUpdate = 4;
        public const byte OnApplicationFocus = 5;
        public const byte OnApplicationPause = 6;
        public const byte OnApplicationQuit = 7;
        public const byte OnJSInit = 8;
        public const byte SocketEvent = 9;
        public const byte OnHttpError = 10;
        public const byte OnCSMessageArrived = 11;

        public const byte UpdateResOver = 20;
        public const byte UIInitOver = 21;
        public const byte EarlyInitOver = 22;
        public const byte UpdateVersion = 23;
        public const byte CheckFile = 24;


        public static void AddCSListener(int rMessageType, int rFilter, MessageHandler rHandler, bool rImmediate)
        {
            MessageDispatcher.AddListener(rMessageType, rFilter, rHandler, rImmediate);
        }

        public static void AddScriptListener(int rMessageType, int rFilter, string scriptClassName, string scriptMethodName, bool rImmediate, bool staticBinding = true)
        {
#if JSSCRIPT
#elif LUASCRIPT
            LuaManager.Require(scriptClassName);
            LuaFunction func = LuaManager.GetFunction(scriptMethodName);
            if (func == null)
            {
                Debugger.LogError("register script listener func is null->" + scriptClassName + "^" + scriptMethodName);
                return;
            }
#endif

            MessageDispatcher.AddListener(rMessageType, rFilter, (message) =>
            {
#if JSSCRIPT
                JsRepresentClass jsRepresent = JsRepresentClassManager.Instance.AllocJsRepresentClass(JSClassName, staticBinding);

                if (message != null)
                {
                    jsRepresent.CallFunctionByFunName(JSMethodName, message.Data);
                }
#elif LUASCRIPT
                LuaManager.CallFunc_VX(func, message.Data);
#endif
            }, rImmediate);
        }

        public static void ScriptSendMessage(string sender, int messageType, int filterName, string data, float delay = 0)
        {
            MessageDispatcher.SendMessage(sender, filterName, messageType, data, delay);
        }

        public static void CSSendMessage(System.Object sender, int messageType, int filterName, System.Object data, float delay = 0)
        {
            MessageDispatcher.SendMessage(sender, filterName, messageType, data, delay);
        }
    }
}