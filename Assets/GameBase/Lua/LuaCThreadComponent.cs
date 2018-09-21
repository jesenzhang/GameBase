using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace GameBase
{
    public class LuaCThreadComponent : MonoBehaviour
    {
        private byte[] recvBuf = null;
        private int channel = -1;
        private bool recving = false;
        private static LuaCThreadComponent instance;
        private System.Action<int, int, byte[], int> processMsg;



        public static void SetProcessMsgFunc(System.Action<int, int, byte[], int> func)
        {
            if (instance == null)
                return;

            instance._SetProcessMsgFunc(func);
        }

        private void _SetProcessMsgFunc(System.Action<int, int, byte[], int> func)
        {
            processMsg = func;
        }

        public static void Send(int toChannel, int gID, int uID, SProto msg)
        {
            if (instance == null)
                return;

            instance._Send(toChannel, gID, uID, msg);
        }

        private void _Send(int toChannel, int gID, int uID, SProto msg)
        {
            LuaCThread.SendMsg(channel, toChannel, gID, uID, msg);
        }

        public static void CreateThread(int channel)
        {
            if (instance == null)
                return;
            instance._CreateThread(channel);
        }

        private void _CreateThread(int channel)
        {
            int re = LuaCThread.CreateCThread(channel, ReceiveBack);
            if (re < 0)
            {
                Debugger.LogError("LuaCThreadComponent create thread failed->%d", channel);
                return;
            }

            this.channel = channel;
        }

        public static void SetRecvBufLen(int buflen)
        {
            if (instance == null)
                return;

            instance._SetRecvBufLen(buflen);
        }

        private void _SetRecvBufLen(int buflen)
        {
            if (buflen <= 0)
                return;
            recvBuf = new byte[buflen];
        }

        private void ReceiveBack(int gID, int uID, int len)
        {
            recving = false;
            if (processMsg == null)
                return;

            if (len < 0)
            {
                return;
            }

            processMsg(gID, uID, recvBuf, len);
            Receive();
        }

        private void Receive()
        {
            if (!recving)
            {
                LuaCThread.Receive(channel, recvBuf);
            }
        }

        void Awake()
        {
            if(instance == null)
                instance = this;
        }

        void Update()
        {
            if (channel < 0)
                return;
            LuaCThread.CThreadRun(channel);
            Receive();
        }
    }
}
