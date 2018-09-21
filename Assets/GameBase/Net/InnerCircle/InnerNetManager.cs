using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static class InnerNetManager
    {
        private static InnerNetNode client;
        private static InnerNetNode server;


        public static void Init()
        {
            client = new InnerNetNode();
            server = new InnerNetNode();

            server.Init(client, 99999999, true);
            client.Init(server, MessagePool.OnCSMessageArrived, true);
        }

        public static void SetClientLuaRecvFunc(string clsName, string funcName)
        {
            if (client == null)
                return;

            client.SetLuaRecvCallFunc(clsName, funcName);
        }

        public static void SetServerLuaRecvFunc(string clsName, string funcName)
        {
            if (server == null)
                return;

            server.SetLuaRecvCallFunc(clsName, funcName);
        }

        public static void ClientSend(byte tid, byte gid, byte uid, byte[] data)
        {
            if (client == null)
                return;

            client.Send(tid, gid, uid, data);
        }

        public static void ServerSend(byte tid, byte gid, byte uid, byte[] data)
        {
            if (server == null)
                return;

            server.Send(tid, gid, uid, data);
        }
    }
}
