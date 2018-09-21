using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public class NetMsg
    {
        private byte tID;
        private byte gID;
        private byte uID;
        private byte[] data;
        private int dataLen;



        internal void init_data(int size)
        {
            data = new byte[size];
        }

        internal int get_capacity()
        {
            if (data == null)
                return 0;
            return data.Length;
        }

        internal void set_tgu(byte tid, byte gid, byte uid)
        {
            tID = tid;
            gID = gid;
            uID = uid;
        }

        internal void copy_data(byte[] buf, int begin, int len)
        {
            dataLen = len;
            if (dataLen > 0)
                Array.Copy(buf, begin, data, 0, dataLen);
        }

        internal byte get_tid()
        {
            return tID;
        }

        internal byte get_gid()
        {
            return gID;
        }

        internal byte get_uid()
        {
            return uID;
        }

        internal byte[] get_data()
        {
            return data;
        }

        internal int get_datalen()
        {
            return dataLen;
        }
    }
}
