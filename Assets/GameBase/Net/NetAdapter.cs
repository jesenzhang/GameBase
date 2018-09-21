using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = System.Object;

namespace GameBase
{
    public class NetAdapter
    {
        public class DeserializeData
        {
            public bool toScript = false;
            public NetMsg data = null;
        }

        private Object LOLK = new System.Object();
        private bool littleEnd = false;

        private string name;

        internal void SetName(string name) 
        {
            this.name = name;
        }

        internal void SetEndianness(bool little)
        {
            littleEnd = little;
        }

        internal bool IsLittleEnd()
        {
            return littleEnd;
        }

        private MemoryStream memoryStream = new MemoryStream();

        private void SerializeHeader(BinaryWriter writer, int len, byte tID, byte gID, byte UID)
        {
            writer.Write(tID);
            writer.Write(len);
            writer.Write(gID);
            writer.Write(UID);
        }

        public byte[] Serialize(byte tid, byte gid, byte uid, byte[] body)
        {
            byte[] headBytes = null;
            byte[] all = null;

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter binaryWriter = new BinaryWriter(ms))
            {
                int len = 0;
                if (body != null && body.Length > 0)
                {
                    len = body.Length;
                    if (!littleEnd)
                        len = len.SwapInt32();
                }

                SerializeHeader(binaryWriter, len, tid, gid, uid);

                headBytes = ms.ToArray();
            }

            if (headBytes != null && headBytes.Length > 0 && body != null)
            {
                all = new byte[headBytes.Length + body.Length];
                Array.Copy(headBytes, 0, all, 0, headBytes.Length);
                Array.Copy(body, 0, all, headBytes.Length, body.Length);
            }

            return all;
        }

        public byte[] Serialize(System.Object ob)
        {
			if (ob == null ) return null;
            byte[] headBytes = null;
            byte[] body = null;
            byte[] all = null;

            lock (LOLK)
            {
                Type type = ob.GetType();
                memoryStream.SetLength(0);
                memoryStream.Position = 0;

                    if (ob == null)
                    {
                        Debugger.LogError("serialize object is null");
                        return null;
                    }

                    Type protoType = ob.GetType();
                    Type enumType = protoType.GetNestedType("ID");
                    byte tID = (byte)(int)enumType.GetField("MRID").GetValue(null);
                    byte gID = (byte)(int)enumType.GetField("GROUPID").GetValue(null);
                    byte uID = (byte)(int)enumType.GetField("UNITID").GetValue(null);

                    //ProtoBuf.Serializer.Serialize(memoryStream, ob);
                    body = memoryStream.ToArray();

                return Serialize(tID, gID, uID, body);
            }
        }

        private StringBuilder sb = new StringBuilder();
        public DeserializeData Deserialize(byte[] datas, int dataLen)
        {
            DeserializeData result = null;

            if (datas != null)
            {
                using (MemoryStream memoryStream = new MemoryStream(datas, 0, dataLen))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    byte tID = binaryReader.ReadByte();
                    int bodyLength = binaryReader.ReadInt32();
                    if (!littleEnd)
                        bodyLength = bodyLength.SwapInt32();

                    byte gID = binaryReader.ReadByte();
                    byte uID = binaryReader.ReadByte();

                    if (Config.Detail_Debug_Log())
                        Debug.Log("---------net adapter deserialize msg data->" + dataLen + "^" + (bodyLength + NetUtils.MSG_HEADER_LEN));

                    if(dataLen == (bodyLength + NetUtils.MSG_HEADER_LEN))
                    {
                        result = new DeserializeData();
                        NetMsg msg = NetMsgPool.GenNetMsg(bodyLength);
                        if (msg == null)
                        {
                            Debugger.LogError("net adapter gen net msg failed->" + bodyLength);
                            return null;
                        }
                        msg.set_tgu(tID, gID, uID);
                        if (Config.Detail_Debug_Log())
                            Debug.LogError("deserialize msg t g u->" + tID + "^" + gID + "^" + uID + "^" + bodyLength);
                        msg.copy_data(datas, NetUtils.MSG_HEADER_LEN, bodyLength);
                        result.data = msg;

                        if (gID <= NetUtils.SCRIPTTOP_GROUP)
                        {
#if JSSCRIPT
                            sb.Remove(0, sb.Length);
                            //sb.Append(protoNumber.ToString());
                            sb.Append(gID);
                            sb.Append(">");
                            sb.Append(uID);
                            sb.Append(">");
                            sb.Append(Convert.ToBase64String(rawBytes));

                            // util.Log.Log(sb.ToString()); 

                            result = new DeserializationData()
                            {
                                Data = sb.ToString(),
                                toScript = true,
                                gID = gID,
                                uID = uID
                                //protoNumber = protoNumber
                            };
#elif LUASCRIPT
                            result.toScript = true;
#endif
                        }
                        else
                        {
                            result.toScript = false;
                        }
                    }
                }
            }

            return result;
        }
    }
}