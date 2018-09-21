using System;
using System.Collections.Generic;
using System.Text;

namespace GameBase
{
    public abstract partial class SProto
    {
        public enum ValueType
        {
            INT32 = 0,
            INT64 = 1,
            FLOAT = 2,
            DOUBLE = 3,
            BOOL = 4,
            STRING = 5,
            BYTE = 6,
            ENUM = 7,
            MESSAGE = 8,
        }

        public enum SerialType
        {
            OPTIONAL = 0,
            REPEATED = 1,
        }

        private const short MESSAGE_OVER_MASK = 30000;

        private static byte[] bufferArr;
        private static ByteBuffer buffer;
        private List<PropertyBase> properties = new List<PropertyBase>();


        public static void Init(int size)
        {
            bufferArr = new byte[size];
            buffer = ByteBuffer.Allocate(bufferArr);
        }


        public byte[] GetMsgBuf()
        {
            return bufferArr;
        }

        private void REG(PropertyBase p)
        {
            p.SetIDX(properties.Count);
            properties.Add(p);
        }

        private void PackMessage(ByteBuffer buf)
        {
            PropertyBase pb;
            for (int i = 0, count = properties.Count; i < count; i++)
            {
                pb = properties[i];
                pb.Pack(buf);
            }

            buf.WriteShort(MESSAGE_OVER_MASK);
        }

        public int Pack()
        {
            buffer.Clear();
            buffer.SetLittleEndian(BitConverter.IsLittleEndian);
            if (BitConverter.IsLittleEndian)
                buffer.WriteByte(1);
            else
                buffer.WriteByte(0);

            PackMessage(buffer);

            return buffer.ReadableBytes();
        }

        private void UnPackProperty(ByteBuffer buf)
        {
            if (buf.ReadableBytes() < 2)
                return;
            int index = buf.ReadUshort();
            if (index == MESSAGE_OVER_MASK)
                return;

            if (index < 0 || index >= properties.Count)
                throw new Exception("proto index is invalid->" + index);
            PropertyBase pb = properties[index];
            pb.UnPack(buf);

            UnPackProperty(buf);
        }

        private void UnPackMessage(ByteBuffer buf)
        {
            UnPackProperty(buf);
        }

        public void UnPack(byte[] arr, int msgLen)
        {
            if (arr == null)
                return;

            buffer.Clear();
            buffer.WriteBytes(arr, msgLen);
            byte v = buffer.ReadByte();
            if (v == 1)
                buffer.SetLittleEndian(true);
            else
                buffer.SetLittleEndian(false);
            UnPackMessage(buffer);
        }
    }
}
