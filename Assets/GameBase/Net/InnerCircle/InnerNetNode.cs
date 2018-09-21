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
    internal class InnerNetNode
    {
        private NetAdapter adapter = null;

        internal InnerNetNode()
        {
        }

        private InnerNetNode netNode = null;
        //------------------------------------------msg pack---------------------------------------------
        private byte[] currentBody = new byte[1024 * 1024];
        private int currentLength = 0;
        private int totalLength = 0;
        private short total = 0;
        private bool packageTrigger = true;
        private byte[] headData = new byte[NetUtils.MSG_HEADER_LEN];
        private int headDataLen = 0;

        private int csNetGID = -1;

        private object lockObj = new object();

#if LUASCRIPT
        private LuaInterface.LuaFunction recvMsgCall = null;
#endif

        private void Clear()
        {
            currentBody = null;
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
                        }

                        if(totalLength > currentBody.Length)
                            currentBody = new byte[totalLength];
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
                        Array.Copy(headData, 0, currentBody, currentLength, headDataLen);
                        currentLength += headDataLen;
                        bufLen -= headDataLen;
                        headDataLen = 0;
                    }
                }
                else
                    bufLen = rlen;

                //Debug.Log("analysis data 10->" + bufLen + "^" + dataEventArgs.Offset + "^" + currentLength);
                Array.Copy(dataEventArgs.Data, dataEventArgs.Offset, currentBody, currentLength, bufLen);
                currentLength += bufLen;
                dataEventArgs.Offset += bufLen;

                //Debugger.Log("analysis data 11->" + totalLength + "^" + currentLength + "^" + rlen + "^" + bufLen);
                if (totalLength > currentLength)
                {
                    //Debug.Log("analysis data 12");
                    packageTrigger = false;
                }
                else if (rlen > bufLen)
                {
                    packageTrigger = true;
                    Deserialize(currentBody, totalLength);
                    Clear();
                    RecvData(dataEventArgs);
                }
                else if (rlen == bufLen)
                {
                    //Debug.Log("analysis data 13");
                    packageTrigger = true;
                    //_dataHandler(currentBody);
                    Deserialize(currentBody, totalLength);
                    Clear();
                }
                else if (totalLength < currentLength)
                {
                    //Debug.LogError("total len < currentLen close->" + TotalLength + "^" + currentLength);
                }
            }
            total++;
        }

        //--------------------------------end msg pack------------------------------------------------------
        public void SetEndianness(bool littleEnd)
        {
            adapter.SetEndianness(littleEnd);
        }

        private bool inited = false;
        public void Init(InnerNetNode node, int cs, bool littleEnd)
        {
            if (!inited)
            {
                netNode = node;
                csNetGID = cs;
                adapter = new NetAdapter();
                adapter.SetEndianness(littleEnd);
                inited = true;
            }
        }


        /*
        internal void Connect(InnerNetNode node)
        {
        }
        */

#if LUASCRIPT
        //internal struct RecvData
        //{
        //    internal InnerNetNode node;
        //    internal NetAdapter.DeserializationData data;
        //}

        internal void SetLuaRecvCallFunc(string className, string funcName)
        {
            LuaManager.Require(className);
            recvMsgCall = LuaManager.GetFunction(funcName);
            //Debugger.Log("set lua call->" + (recvMsgCall == null));
        }

        private void CallLuaRecvFunc(NetAdapter.DeserializeData data)
        {
            //Debugger.Log("recv msg call lua func->" + (recvMsgCall == null));
            if (recvMsgCall != null)
                LuaManager.CallFunc_V(recvMsgCall, data.data.get_tid(), data.data.get_gid(), data.data.get_uid(), data.data.get_data(), data.data.get_datalen());
            //Debugger.Log("recv msg call lua func 1");
        }
#endif

        private void Deserialize(byte[] dataBytes, int dataLen)
        {
            NetAdapter.DeserializeData deserializeData = adapter.Deserialize(dataBytes, dataLen);
            if (deserializeData != null)
            {
                //Debugger.Log("recv pmsg 2->" + deSerializationData.toScript + "^" + deSerializationData.Data);
                if (deserializeData.toScript)
                {
#if JSSCRIPT
                    MessagePool.ScriptSendMessage(null, MessagePool.OnNetMessageArrived, Message.FilterTypeNothing, (string)deSerializationData.Data);
#elif LUASCRIPT
                    //MessagePool.CSSendMessage(null, MessagePool.OnNetMessageArrived, Message.FilterTypeNothing, new RecvData() { node = this, data = deSerializationData });
                    ThreadTask.QueueOnMainThread(()=> 
                    {
                        CallLuaRecvFunc(deserializeData);
                        NetMsgPool.RecycleMsg(deserializeData.data);
                        deserializeData.data = null;
                    });
#endif
                }
                else
                {
                    //MessagePool.CSSendMessage(null, csNetGID, Message.FilterTypeNothing, deserializeData.data);
                    NetMsgPool.RecycleMsg(deserializeData.data);
                    deserializeData.data = null;
                }
            }
        }

        public void ReceiveData(byte[] data)
        {
            DataEventArgs arg = new DataEventArgs();
            arg.Data = data;
            arg.Length = data.Length;
            arg.Offset = 0;

            OnDataReceive(null, arg);
        }

        private void OnDataReceive(object sender, DataEventArgs dataEventArgs)
        {
            lock (lockObj)
            {
                RecvData(dataEventArgs);
            }
        }

        /*
        private void onConnected(object sender, EventArgs eventArgs)
        {
            MessagePool.ScriptSendMessage("Socket", MessagePool.SocketEvent, 1, "0");
        }

        private void onClosed(object sender, EventArgs eventArgs)
        {
        }
        private void onError(object sender, EventArgs eventArgs)
        {
        }
        */

        public void Close()
        {
        }

        public void Send(byte tid, byte gid, byte uid, byte[] data)
        {
            byte[] dataBytes = adapter.Serialize(tid, gid, uid, data);
            if (dataBytes != null && dataBytes.Length > 0)
            {
                if (netNode != null)
                    netNode.ReceiveData(dataBytes);
            }
            else
                Debugger.Log("tcp send msg is null");
        }
    }
}