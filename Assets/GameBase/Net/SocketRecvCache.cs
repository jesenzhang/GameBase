using System;


namespace GameBase
{
    public class SocketRecvCache
    {
        private byte[] cacheData;
        private ArraySegment<byte> cacheSegment;


        public SocketRecvCache(int capacity)
        {
            cacheData = new byte[capacity];
        }
    }
}
