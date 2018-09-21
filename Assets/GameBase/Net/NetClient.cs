#define LUASCRIPT
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;

namespace GameBase
{
    public class NetClient
    {
        private int netClientId=-1;

        private enum SocketStatus : short
        {
            NotConnet,
            Ready,
        }

        private NetAdapter adapter = null;

        private NetClient()
        {
        }

        public bool IsSocketOK()
        {
            return this.socketStatus == SocketStatus.Ready;
        }

        //------------------------------------------msg pack---------------------------------------------
        private byte[] cacheRecvData = new byte[1024 * 1024];
        private int currentLength = 0;
        private int totalLength = 0;
        private short total = 0;
        private bool packageTrigger = true;
        private byte[] headData = new byte[NetUtils.MSG_HEADER_LEN];
        private int headDataLen = 0;

#if LUASCRIPT
        private LuaInterface.LuaFunction recvMsgCall = null;
#endif


        private void Clear()
        {
            currentLength = 0;
            totalLength = 0;
            total = 0;
        }

        private void RecvData(DataEventArgs dataEventArgs)
        {
            int rlen = dataEventArgs.Length - dataEventArgs.Offset;

            if (rlen > 0)
            {
                if (packageTrigger)
                {
                    /// annalyze the package head
                    if (rlen >= NetUtils.MSG_HEADER_LEN)
                    {
                        Array.Copy(dataEventArgs.Data, dataEventArgs.Offset, headData, headDataLen, NetUtils.MSG_HEADER_LEN - headDataLen);

                        using (MemoryStream ms = new MemoryStream(headData))
                        {
                            using (BinaryReader br = new BinaryReader(ms))
                            {
                                br.ReadByte();
                                int msgLen = br.ReadInt32();
                                if (!adapter.IsLittleEnd())
                                    msgLen = msgLen.SwapInt32();

                                totalLength = NetUtils.MSG_HEADER_LEN + msgLen;
                            }
                        }

                        if (totalLength <= 0)
                        {
                            Debugger.LogError("total len <= 0 close->" + totalLength);
                            onClosed(null, null);
                        }

                        if(totalLength > cacheRecvData.Length)
                            cacheRecvData = new byte[totalLength];
                    }
                }

                if (totalLength <= 0)
                {
                    headDataLen = dataEventArgs.Length - dataEventArgs.Offset;
                    Array.Copy(dataEventArgs.Data, dataEventArgs.Offset, headData, 0, headDataLen);
                    return;
                }

                int bufLen = 0;
                int blen = totalLength - currentLength;
                if (rlen >= blen)
                {
                    bufLen = blen;

                    if (headDataLen > 0) 
                    {
                        Array.Copy(headData, 0, cacheRecvData, currentLength, headDataLen);
                        currentLength += headDataLen;
                        bufLen -= headDataLen;
                        headDataLen = 0;
                    }
                }
                else
                    bufLen = rlen;

                Array.Copy(dataEventArgs.Data, dataEventArgs.Offset, cacheRecvData, currentLength, bufLen);
                currentLength += bufLen;
                dataEventArgs.Offset += bufLen;

                if (totalLength > currentLength)
                {
                    packageTrigger = false;
                }
                else if (rlen > bufLen)
                {
                    packageTrigger = true;
                    Deserialize(cacheRecvData, totalLength);
                    Clear();
                    RecvData(dataEventArgs);
                }
                else if (rlen == bufLen)
                {
                    packageTrigger = true;
                    Deserialize(cacheRecvData, totalLength);
                    Clear();
                }
                else if (totalLength < currentLength)
                {
                    onClosed(null, null);
                }
            }
            total++;
        }

        //--------------------------------end msg pack------------------------------------------------------

        private string currentHost = "";
        private int currentPort = 0;

        private SocketStatus socketStatus = SocketStatus.NotConnet;

        private AsyncTcpSession tcpSession = null;

        private System.Object lockOj = new System.Object();




        public void SetEndianness(bool littleEnd)
        {
            adapter.SetEndianness(littleEnd);
        }

        public void ReConnect()
        {
            Connect();
        }

        private bool inited = false;
        public void Init(string host, int port, int index, bool littleEnd)
        {
            try
            {
                //if (inited)
                if (tcpSession != null)
                {
                    tcpSession.Close();
                    tcpSession = null;
                }

                //if (!inited)
                {
                    adapter = new NetAdapter();
                    adapter.SetEndianness(littleEnd);
                    netClientId = index;
                    socketStatus = SocketStatus.NotConnet;
                    currentHost = host;
                    currentPort = port;
                    IPAddress ipEndPoint = IPAddress.Parse(currentHost);
                    IPEndPoint ipEnd = new IPEndPoint(ipEndPoint, currentPort);
                    tcpSession = new AsyncTcpSession(ipEnd);
                    tcpSession.Connected += onConnected;
                    tcpSession.Closed += onClosed;
                    tcpSession.Error += onError;
                    tcpSession.DataReceived += OnDataReceive;
                }
            }
            catch (System.Exception e)
            {
                Debugger.LogError("net client init exception->" + host + "^" + port + "^" + index + "^" + littleEnd + "^" + e.ToString());
            }

            inited = true;
        }

        internal static NetClient GenNetClient()
        {
            return new GameBase.NetClient();
        }

        internal static NetClient CloneNetClient(NetClient c)
        {
            NetClient nc = new NetClient();
            nc.Init(c.currentHost, c.currentPort, c.netClientId, c.adapter.IsLittleEnd());
            nc.recvMsgCall = c.recvMsgCall;
            return nc;
        }

        public override string ToString()
        {
            return "netClientId>>" + this.netClientId + ">>currentHost>>" + this.currentHost + ">>port>>" + currentPort +
                   ">>status" + Enum.GetName(typeof(SocketStatus), this.socketStatus);
        }

        internal void Connect()
        {
            Debugger.Log("begin connect->" + netClientId + "^" + currentHost + "^" + currentPort);
            if (inited && socketStatus == SocketStatus.NotConnet)
            {
                tcpSession.Connect();
            }
        }

#if LUASCRIPT
        internal void SetLuaRecvCallFunc(string className, string funcName)
        {
            LuaManager.Require(className);
            recvMsgCall = LuaManager.GetFunction(funcName);
        }

        private void CallLuaRecvFunc(NetAdapter.DeserializeData data)
        {
            if (Config.Detail_Debug_Log())
                Debug.Log("recv server msg push to lua->" + data.data.get_tid() + "^" + data.data.get_gid() + "^" + data.data.get_uid() + "^" +data.data.get_datalen() +"^" + (recvMsgCall == null));
            if (recvMsgCall != null)
                LuaManager.CallFunc_V(recvMsgCall, data.data.get_tid(), data.data.get_gid(), data.data.get_uid(), data.data.get_data(), data.data.get_datalen());
        }
#endif

        private void Deserialize(byte[] dataBytes, int dataLen)
        {
            if (Config.Detail_Debug_Log())
                Debug.Log("---------net client deserialize msg data 1->" + dataBytes + "^" +dataLen);

            if (Config.Detail_Debug_Log())
            {
                Debug.LogError("/////////////////////////////////////////////////////////////////////");
                StringBuilder sb = new StringBuilder();
                if (dataBytes != null)
                {
                    int num = 0;
                    for (int i = 0; i < dataLen; i++)
                    {
                        sb.Append(dataBytes[i].ToString("X2"));
                        sb.Append(" ");
                        if (num >= 10)
                        {
                            sb.Append("\r\n");
                            num = 0;
                        }
                        num++;
                    }

                    Debug.LogError(sb.ToString());
                }
                Debug.LogError("*********************************************************************");
            }

            NetAdapter.DeserializeData deserializeData = adapter.Deserialize(dataBytes, dataLen);

            if(Config.Detail_Debug_Log())
                Debug.Log("---------net client deserialize msg data 2->" + (deserializeData == null));

            if (deserializeData != null)
            {
                if (deserializeData.toScript)
                {
#if JSSCRIPT
                    MessagePool.ScriptSendMessage(null, MessagePool.OnNetMessageArrived, Message.FilterTypeNothing, (string)deSerializationData.Data);
#elif LUASCRIPT
                    if (Config.Detail_Debug_Log())
                    {
                        Debug.Log("---------net client deserialize msg data 3->" + (deserializeData.data == null));

                        if (deserializeData.data != null)
                            Debug.Log("----------------------net client deserialize msg data 10->" + deserializeData.data.get_tid() + "^" + deserializeData.data.get_gid() + "^" + deserializeData.data.get_uid() + "^" + deserializeData.data.get_datalen());
                    }


                    ThreadTask.QueueOnMainThread(()=>
                    {
                        if (Config.Detail_Debug_Log())
                        {
                            Debug.Log("---------net client deserialize msg data 4->" + (deserializeData.data == null ? -1 : (deserializeData.data.get_data() == null ? -2 : 1))); 
                            if (deserializeData.data != null)
                                Debug.Log("----------------------net client deserialize msg data 11->" + deserializeData.data.get_tid() + "^" + deserializeData.data.get_gid() + "^" + deserializeData.data.get_uid() + "^" + deserializeData.data.get_datalen());
                        }

                        CallLuaRecvFunc(deserializeData);
                        NetMsgPool.RecycleMsg(deserializeData.data);
                        deserializeData.data = null;
                    });
#endif
                }
                else
                {
                     NetMsgPool.RecycleMsg(deserializeData.data);
                     deserializeData.data = null;
                }
            }
        }

        private void OnDataReceive(object sender, DataEventArgs dataEventArgs)
        {
            lock (lockOj)
            {
                RecvData(dataEventArgs);
            }
        }

        private void onConnected(object sender, EventArgs eventArgs)
        {
            socketStatus = SocketStatus.Ready;
            MessagePool.ScriptSendMessage("Socket", MessagePool.SocketEvent, 1, netClientId.ToString());
            Debugger.Log("Net Connect Success->" +ToString());
        }

        private void onClosed(object sender, EventArgs eventArgs)
        {
            socketStatus = SocketStatus.NotConnet;
            MessagePool.ScriptSendMessage("Socket", MessagePool.SocketEvent, 2, netClientId.ToString());
            Debugger.Log("Net Close->" + ToString());
        }
        private void onError(object sender, EventArgs eventArgs)
        {
            socketStatus = SocketStatus.NotConnet;
            MessagePool.ScriptSendMessage("Socket", MessagePool.SocketEvent, 3, netClientId.ToString());
            Debugger.LogError("Net Error" + ToString());
        }


        public void Close()
        {
            if (Config.Detail_Debug_Log())
                Debug.Log("---------net client close");

            if (tcpSession != null)
            {
                tcpSession.Close();
                socketStatus = SocketStatus.NotConnet;
            }
        }

        public void SendToSocketServer(byte tid, byte gid, byte uid, byte[] body)
        {
            if (Config.Detail_Debug_Log())
                Debug.Log("---------send to socket server 1->" + tid + "^" + gid + "^" + uid + "^" + (body == null ? -1 : body.Length));
            if (socketStatus != SocketStatus.Ready)
            {
                Debugger.LogError("socket is not connected");
                return;
            }

            byte[] dataBytes = adapter.Serialize(tid, gid, uid, body);
            if (Config.Detail_Debug_Log())
                Debug.Log("---------send to socket server 2->" + (dataBytes == null ? -1 : dataBytes.Length));
            SendData(dataBytes); 
        }

        private void SendData(byte[] dataBytes)
        {
            if(dataBytes != null && dataBytes.Length > 0)
            {
                if (socketStatus == SocketStatus.Ready)
                {
                    tcpSession.Send(dataBytes, 0, dataBytes.Length);
                }
            }
            else
                Debugger.Log("tcp send msg is null");
        }

        public void SendToSocketServer(System.Object ob)
        {
            if (socketStatus != SocketStatus.Ready)
            {
                Debugger.LogError("socket is not connected");
                return;
            }

            byte[] dataBytes = adapter.Serialize(ob);
            SendData(dataBytes); 
        }
    }
}