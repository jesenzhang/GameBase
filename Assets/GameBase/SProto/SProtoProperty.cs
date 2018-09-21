using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public partial class SProto
    {
        private static void CheckLength(ByteBuffer buf, int size, string type)
        {
            int remain = buf.ReadableBytes();
            if (remain < 1)
                throw new Exception(string.Format("sproto parse {0} error : len->{1}", type, remain));
        }

        private static void ValueToBuffer(ByteBuffer buf, ValueType vt, object v)
        {
            if (vt == ValueType.BOOL)
            {
                bool tv = (bool)v;
                if (tv)
                    buf.WriteByte(1);
                else
                    buf.WriteByte(0);
            }
            else if (vt == ValueType.BYTE)
            {
                buf.WriteByte((byte)v);
            }
            else if (vt == ValueType.DOUBLE)
            {
                buf.WriteDouble((double)v);
            }
            else if (vt == ValueType.FLOAT)
            {
                buf.WriteFloat((float)v);
            }
            else if (vt == ValueType.INT32)
            {
                buf.WriteInt((int)v);
            }
            else if (vt == ValueType.INT64)
            {
                buf.WriteLong((long)v);
            }
            else if (vt == ValueType.STRING)
            {
                byte[] arr = System.Text.Encoding.UTF8.GetBytes((string)v);
                buf.WriteInt(arr.Length);
                buf.WriteBytes(arr);
            }
            else if (vt == ValueType.ENUM)
            {
                buf.WriteInt((int)v);
            }
            else if (vt == ValueType.MESSAGE)
            {
                SProto sp = (SProto)v;
                sp.PackMessage(buf);
            }
            else
                throw new Exception("sproto parse value error->" + vt);
        }

        private static T BufferToMessage<T>(ByteBuffer buf) where T : SProto
        {
            T t = default (T);
            t.UnPackMessage(buf);

            return t;
        }

        private static object BufferToValue(ByteBuffer buf, ValueType vt)
        {
            if (vt == ValueType.BOOL)
            {
                CheckLength(buf, 1, "bool");
                byte b = buf.ReadByte();
                if (b == 1)
                    return true;
                else
                    return false;
            }
            else if (vt == ValueType.BYTE)
            {
                CheckLength(buf, 1, "byte");
                return buf.ReadByte();
            }
            else if (vt == ValueType.DOUBLE)
            {
                CheckLength(buf, 8, "double");
                return buf.ReadDouble();
            }
            else if (vt == ValueType.FLOAT)
            {
                CheckLength(buf, 4, "float");
                return buf.ReadFloat();
            }
            else if (vt == ValueType.INT32)
            {
                CheckLength(buf, 4, "int");
                return buf.ReadInt();
            }
            else if (vt == ValueType.INT64)
            {
                CheckLength(buf, 8, "int64");
                return buf.ReadLong();
            }
            else if (vt == ValueType.STRING)
            {
                CheckLength(buf, 4, "string len");
                int l = buf.ReadInt();
                CheckLength(buf, l, "string");
                byte[] arr = new byte[l];
                buf.ReadBytes(arr, 0, l);
                return System.Text.Encoding.UTF8.GetString(arr);
            }
            else
                throw new Exception("sproto parse value error->" + vt);
        }

        public class PropertyBase
        {
            private int index = -1;
            private SerialType serialType = SerialType.OPTIONAL;
            private ValueType valueType = ValueType.INT32;

            public PropertyBase(SProto sp)
            {
                sp.REG(this);
            }

            internal virtual void Pack(ByteBuffer buf)
            {
                buf.WriteUshort((ushort)index);
            }
            internal virtual void UnPack(ByteBuffer buf)
            {
            }

            internal SerialType GetSerialType()
            {
                return serialType;
            }
            internal void SetSerialType(SerialType st)
            {
                serialType = st;
            }

            internal ValueType GetValueType()
            {
                return valueType;
            }

            private void SetValueType(ValueType vt)
            {
                valueType = vt;
            }

            internal int GetIDX()
            {
                return index;
            }

            internal void SetIDX(int v)
            {
                index = v;
            }

            protected void CheckValueType(System.Type t)
            {
                if (t == typeof(int))
                    SetValueType(ValueType.INT32);
                else if (t == typeof(long))
                    SetValueType(ValueType.INT64);
                else if (t == typeof(float))
                    SetValueType(ValueType.FLOAT);
                else if (t == typeof(double))
                    SetValueType(ValueType.DOUBLE);
                else if (t == typeof(bool))
                    SetValueType(ValueType.BOOL);
                else if (t == typeof(byte))
                    SetValueType(ValueType.BYTE);
                else if (t == typeof(string))
                    SetValueType(ValueType.STRING);
                else
                    throw new Exception("sproto invalid type->" + t);
            }
        }

        public class OptionalMessage<T> : PropertyBase where T : SProto
        {
            public T data;

            public OptionalMessage(SProto sp) : base(sp)
            {
            }

            internal override void Pack(ByteBuffer buf)
            {
                base.Pack(buf);
                ValueToBuffer(buf, GetValueType(), data);
            }

            internal override void UnPack(ByteBuffer buf)
            {
                base.UnPack(buf);
                data = BufferToMessage<T>(buf);
            }
        }

        public class RepeatedMessage<T> : PropertyBase where T : SProto
        {
            private List<T> list = new List<T>();


            public RepeatedMessage(SProto sp) : base(sp)
            {
            }

            public T this[int index]
            {
                get
                {
                    if (list != null && list.Count > index)
                    {
                        return list[index];
                    }
                    return default(T);
                }
                set
                {
                    if (list != null && list.Count > index)
                    {
                        list[index] = value;
                    }
                }
            }

            public void Add(T value)
            {
                if (list != null)
                {
                    list.Add(value);
                }
            }

            public void RemoveAt(int value)
            {
                if (list != null)
                {
                    list.RemoveAt(value);
                }
            }

            public void Clear()
            {
                if (list != null)
                {
                    list.Clear();
                }
            }

            public int Count
            {
                get
                {
                    if (list != null)
                    {
                        return list.Count;
                    }
                    return -1;
                }
            }

            public bool Contains(T value)
            {
                if (list != null)
                {
                    return list.Contains(value);
                }
                return false;
            }

            internal override void Pack(ByteBuffer buf)
            {
                base.Pack(buf);
                int jcount = Count;
                buf.WriteInt(jcount);
                ValueType vt = GetValueType();
                for (int j = 0; j < jcount; j++)
                {
                    ValueToBuffer(buf, vt, this[j]);
                }
            }

            internal override void UnPack(ByteBuffer buf)
            {
                base.UnPack(buf);
                CheckLength(buf, 4, "unpack repeated");
                int count = buf.ReadInt();
                for (int i = 0; i < count; i++)
                {
                    list.Add(BufferToMessage<T>(buf));
                }
            }
        }

        public class Optional<T> : PropertyBase
        {
            public T data;

            public Optional(SProto sp) : base(sp)
            {
                CheckValueType(typeof(T));
            }

            internal override void Pack(ByteBuffer buf)
            {
                base.Pack(buf);
                ValueToBuffer(buf, GetValueType(), data);
            }

            internal override void UnPack(ByteBuffer buf)
            {
                base.UnPack(buf);
                data = (T)BufferToValue(buf, GetValueType());
            }
        }

        public class Repeated<T> : PropertyBase
        {
            private List<T> list = new List<T>();

            public Repeated(SProto sp) : base(sp)
            {
                CheckValueType(typeof(T));
            }

            public T this[int index]
            {
                get
                {
                    if (list != null && list.Count > index)
                    {
                        return list[index];
                    }
                    return default(T);
                }
                set
                {
                    if (list != null && list.Count > index)
                    {
                        list[index] = value;
                    }
                }
            }

            public void Add(T value)
            {
                if (list != null)
                {
                    list.Add(value);
                }
            }

            public void RemoveAt(int value)
            {
                if (list != null)
                {
                    list.RemoveAt(value);
                }
            }

            public void Clear()
            {
                if (list != null)
                {
                    list.Clear();
                }
            }

            public int Count
            {
                get
                {
                    if (list != null)
                    {
                        return list.Count;
                    }
                    return -1;
                }
            }

            public bool Contains(T value)
            {
                if (list != null)
                {
                    return list.Contains(value);
                }
                return false;
            }

            internal override void Pack(ByteBuffer buf)
            {
                base.Pack(buf);
                int jcount = Count;
                buf.WriteInt(jcount);
                ValueType vt = GetValueType();
                for (int j = 0; j < jcount; j++)
                {
                    ValueToBuffer(buf, vt, this[j]);
                }
            }

            internal override void UnPack(ByteBuffer buf)
            {
                base.UnPack(buf);
                CheckLength(buf, 4, "unpack repeated");
                int count = buf.ReadInt();
                ValueType vt = GetValueType();
                for (int i = 0; i < count; i++)
                {
                    list.Add((T)BufferToValue(buf, vt));
                }
            }
        }
    }
}
