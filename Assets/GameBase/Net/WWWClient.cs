
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    class WWWClient
    {
        private static NetAdapter adapter = new NetAdapter();
        private static LuaInterface.LuaFunction protoBufCall = null;


        internal static void HttpSend(string method, string url, byte[] data, bool strResult, LuaInterface.LuaFunction func)
        {
            if (method == "GET")
            {
                TaskManager.CreateTask(HttpGet(url, strResult, func), null).Start();
            }
            else if (method == "POST")
            {
                TaskManager.CreateTask(HttpPost(url, data, strResult, func), null).Start();
            }
            else
                Debugger.LogError("invalid http method->" + method);
        }


        private static IEnumerator HttpPost(string url, byte[] data, bool strResult, LuaInterface.LuaFunction func)
        {
            WWW www = new WWW(url, data);
            yield return www;

            if (www.error != null)
            {
                Debugger.LogError("n http post send error->" + www.error);
            }
            else
            {
                if (www.bytes != null)
                {
                    if (strResult)
                    {
                        string tdata = System.Text.Encoding.UTF8.GetString(www.bytes);
                        LuaManager.CallFunc_V(func, tdata);
                    }
                    else
                    {
                        LuaManager.CallFunc_V(func, www.bytes);
                    }
                }
            }
        }

        private static IEnumerator HttpGet(string url, bool strResult, LuaInterface.LuaFunction func)
        {
            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                Debugger.LogError("n http get send error->" + www.error);
            }
            else
            {
                if (www.bytes != null)
                {
                    if (strResult)
                    {
                        string data = System.Text.Encoding.UTF8.GetString(www.bytes);
                        Debug.LogError("http get re->" + data);
                        LuaManager.CallFunc_V(func, data);
                    }
                    else
                    {
                        LuaManager.CallFunc_V(func, www.bytes);
                    }
                }
            }

            www.Dispose();
        }

        internal static void HttpSendBackProtoBuf(string method, string url, byte[] data)
        {
            if (method == "GET")
                TaskManager.CreateTask(HttpGetSendBackProtoBuf(url), null).Start();
            else if (method == "POST")
            {
                TaskManager.CreateTask(HttpPostSendBackProtoBuf(url, data), null).Start();
            }
            else
                Debugger.LogError("invalid http method->" + method);
        }

        internal static void HttpGetSendBackString(string url, string ob, string className, string funcName)
        {
            TaskManager.CreateTask(_HttpGetSendBackString(url, ob, className, funcName), null).Start();
        }

        internal static void SetHttpBackProtoBufCall(string className, string funcName)
        {
            LuaManager.Require(className);
            protoBufCall = LuaManager.GetFunction(funcName);
        }

        private static void CallLuaRecvFunc(NetAdapter.DeserializeData data)
        {
            if (protoBufCall != null)
                LuaManager.CallFunc_V(protoBufCall, data.data.get_tid(), data.data.get_gid(), data.data.get_uid(), data.data.get_data(), data.data.get_datalen());
        }

        private static IEnumerator HttpPostSendBackProtoBuf(string url, byte[] data)
        {
            WWW www = new WWW(url, data);
            yield return www;

            if (www.error != null)
            {
                Debugger.LogError("http post send error->" + www.error);
            }
            else
            {
                if (www.bytes != null)
                {
                    NetAdapter.DeserializeData recvData = adapter.Deserialize(www.bytes, www.bytes.Length);
                    CallLuaRecvFunc(recvData);
                }
            }

            www.Dispose();
        }

        private static IEnumerator HttpGetSendBackProtoBuf(string url)
        {
            WWW www = new WWW(url);
            yield return www;

            if (www.error != null)
            {
                Debugger.LogError("http get send error->" + www.error);
            }
            else
            {
                if (www.bytes != null)
                {
                    NetAdapter.DeserializeData recvData = adapter.Deserialize(www.bytes, www.bytes.Length);
                    CallLuaRecvFunc(recvData);
                }
            }

            www.Dispose();
        }

        private static IEnumerator _HttpGetSendBackString(string url, string ob, string className, string functionName)
        {
            WWW www = new WWW(url + "?" + ob);
            yield return www;

            if (www.error != null)
            {
                Debugger.LogError("http get send error->" + www.error);
            }
            else
            {
                if (www.bytes != null)
                {
                    string data = System.Text.Encoding.UTF8.GetString(www.bytes);

                    if (!string.IsNullOrEmpty(data))
                    {
#if JSSCRIPT
                                    JsRepresentClass jsRepresentClass =
                                        JsRepresentClassManager.Instance.AllocJsRepresentClass(className, true);
                                    jsRepresentClass.CallFunctionByFunName(functionName, data);
#elif LUASCRIPT
                        LuaManager.Require(className);
                        LuaInterface.LuaFunction func = LuaManager.GetFunction(functionName);
                        LuaManager.CallFunc_VX(func, data);
#endif
                    }
                }
                else
                    Debugger.LogError("http get response data is null");
            }

            www.Dispose();
        }
    }
}
