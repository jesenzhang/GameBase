#define LUASCRIPT
using System.Net;
using GameBase;
using UnityEngine;
using System.Collections;

namespace GameBase
{
    public static class NetworkManager 
    {
        private static NetClient[] clients = null;

        public static void Init(int num) 
        {
            NetMsgPool.Init(1024 * 1024 * 4);
            if (clients != null)
            {
                for (int i = 0; i < clients.Length; i++)
                {
                    if (clients[i] != null)
                        clients[i].Close();
                }
            }

            clients = new NetClient[num];

            for (int i = 0; i < num; i++)
            {
                clients[i] = NetClient.GenNetClient();
            }
        }

        public static void SetScriptTopGroup(int g)
        {
            NetUtils.SetScriptTopGroup(g);
        }

        public static void ReConnectSocketClient(int index)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
            {
                Debugger.LogError("connect index is invalid->" + index + "^" + clients.Length);
                return;
            }
            NetClient netClient = clients[index];
            netClient.Close();
            clients[index] = NetClient.CloneNetClient(netClient);
            ConnectSocketClient(index);
        }

        public static void ConnectSocketClient(int index)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
            {
                Debugger.LogError("connect index is invalid->" + index + "^" + clients.Length);
                return;
            }
            else
            {
                NetClient netClient = clients[index];
                if (netClient != null && !netClient.IsSocketOK())
                {
                    netClient.Connect();
                }
                else
                {
                   Debugger.LogError("need to register socket client first");
                }
            }
        }

        public static void RegisterSocketClient(string host, int port, int index, bool islittleEnd) 
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
            {
                Debugger.LogError("connect index is invalid->" + index + "^" + clients.Length);
                return;
            }

            {
                NetClient netClient=  clients[index];
                if (netClient == null)
                {
                    netClient = NetClient.GenNetClient();
                    clients[index] = netClient;
                }
                if (netClient.IsSocketOK())
                {
                    netClient.Close();
                }
                clients[index].Init(host, port, index, islittleEnd);
            }
        }

        public static void ScriptSendToSocketServer(int index, int tid, int gid, int uid, byte[] body) 
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length) 
            {
                Debugger.LogError("script send client index is invalid->" + index + clients.Length);
                return;
            }
            NetClient netClient=clients[index];
            if (netClient != null && netClient.IsSocketOK())
            {
                netClient.SendToSocketServer((byte)tid, (byte)gid, (byte)uid, body);
            }
            else
            {
                Debugger.LogError("net client is not available->" + index);
            }
        }

        public static void SendToSocketServer(int index, System.Object msg)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
            {
                Debugger.LogError("send client index is invalid->" + index + "^" + clients.Length);
                return;
            }
            NetClient netClient = clients[index];
            if (netClient != null && netClient.IsSocketOK())
            {
                netClient.SendToSocketServer(msg);
            }
            else
            {
                Debugger.LogError("net client is not available->" + index);
            }
        }

        public static void ScriptHttpSendBackProtoBuf(string method, string url, byte[] data)
        {
            WWWClient.HttpSendBackProtoBuf(method, url, data);
        }

        public static void ScriptHttpSend(string method, string url, byte[] data, bool strResult, LuaInterface.LuaFunction func)
        {
            WWWClient.HttpSend(method, url, data, strResult, func);
        }

        public static void ScriptHttpGetSendBackString(string url, string ob, string className, string functionName)
        {
            WWWClient.HttpGetSendBackString(url, ob, className, functionName);
        }

        public static void SetEndianness(int index, bool littleEnd)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
            {
                Debug.LogError("sent endianness client index is invalid->" + index + clients.Length);
                return;
            }

            clients[index].SetEndianness(littleEnd);
        }

        public static void Close(int index)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
                return;

            if (clients[index] != null)
            {
                if (Config.Detail_Debug_Log())
                    Debug.LogError("initiative close net client->" + index);
                clients[index].Close();
                clients[index] = null;
            }
        }

        public static void CloseAll()
        {
            if (clients == null)
                return;
            if (Config.Detail_Debug_Log())
                    Debug.LogError("initiative close all net client");
            for (int i = 0, count = clients.Length; i < count; i++)
                Close(i);
        }

#if LUASCRIPT
        public static void SetSocketClientLuaDataRecvCall(int index, string className, string funcName)
        {
            if (clients == null)
                return;
            if (index < 0 || index >= clients.Length)
                return;
            if (clients[index] != null)
                clients[index].SetLuaRecvCallFunc(className, funcName);
        }

        public static void SetHttpLuaProtoBufRecvCall(string className, string funcName)
        {
            WWWClient.SetHttpBackProtoBufCall(className, funcName);
        }
#endif
    }
}
