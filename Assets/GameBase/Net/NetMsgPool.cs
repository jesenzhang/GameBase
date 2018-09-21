using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static class NetMsgPool
    {
        private const int CAPACITY = 10;
        private static List<SecurityQueue<NetMsg>> idlePool = new List<SecurityQueue<NetMsg>>(CAPACITY);
        private static List<int> sizes = new List<int>(CAPACITY);
        private static int maxCapacity;
        private static bool inited = false;


        public static void Init(int maxCap)
        {
            if (inited)
                return;
            inited = true;

            sizes.Add(64);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(512);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(1024);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(1024 * 64);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(1024 * 512);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(1024 * 1024);
            idlePool.Add(new SecurityQueue<NetMsg>());

            sizes.Add(1024 * 1024 * 2);
            idlePool.Add(new SecurityQueue<NetMsg>());

            if (maxCap >= (1024 * 1024 * 2))
            {
                sizes.Add(maxCap);
                idlePool.Add(new SecurityQueue<NetMsg>());
            }

            maxCapacity = maxCap;
        }

        internal static NetMsg GenNetMsg(int len)
        {
            if (len < 0)
                return null;

            int capacity = 0;
            int cap = 1024;
            int capacityIndex = -1;
            for (int i = 0, count = sizes.Count; i < count; i++)
            {
                cap = sizes[i];
                if (cap >= len)
                {
                    capacity = cap;
                    capacityIndex = i;
                    break;
                }
            }

            if (capacity == 0)
                return null;

            SecurityQueue<NetMsg> idle = idlePool[capacityIndex];
            NetMsg msg;
            bool re = idle.Dequeue(out msg);

            if (!re)
            {
                msg = new NetMsg();
                msg.init_data(capacity);
            }

            return msg;
        }

        internal static void RecycleMsg(NetMsg msg)
        {
            if (msg == null)
                return;

            int cap;
            int capacity = msg.get_capacity();
            int capacityIndex = -1;
            for (int i = 0, count = sizes.Count(); i < count; i++)
            {
                cap = sizes[i];
                if (cap >= capacity)
                {
                    capacityIndex = i;
                    break;
                }
            }

            if (capacity >= 0)
            {
                SecurityQueue<NetMsg> idle = idlePool[capacityIndex];
                idle.Enqueue(msg);
            }
            else
            {
                Debugger.LogError("recycle net msg error");
            }
        }
    }
}
