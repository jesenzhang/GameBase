
using System;
using SuperSocket.ClientEngine;

namespace GameBase
{
    public class SocketSendCache
    {
        private byte[] cacheData;
        private ArraySegment<byte> cacheSegment;
        private int cacheLen;


        public SocketSendCache(int capacity)
        {
            cacheData = new byte[capacity];
            cacheSegment = new ArraySegment<byte>(cacheData);
            cacheLen = 0;
        }

        public void SendCache(ClientSession session)
        {
            if (session == null)
                return;
            if (session.TrySend(cacheSegment))
            {
            }
        }
    }
}
