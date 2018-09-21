using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public class CachedByteArray
    {
        class CachedData
        {
            public byte[] data;
            public bool isUsing;
        }

        private List<CachedData> cache = new List<CachedData>();
        private int bufferSize;


        public CachedByteArray(int size)
        {
            bufferSize = size;
        }

        public void RecycleByteBuffer(int id)
        {
            if (id < 0 || id >= cache.Count)
            {
                Debugger.LogError("recycle bytebuffer error->" + id + "^" + cache.Count);
                return;
            }

            cache[id].isUsing = false;
        }

        public int GetByteBuffer(out byte[] buffer)
        {
            if (bufferSize <= 0)
            {
                buffer = null;
                return -1;
            }
            CachedData data;
            int index = -1;
            for (int i = 0, count = cache.Count; i < count; i++)
            {
                data = cache[i];
                if (data.isUsing)
                    continue;

                index = i;
                break;
            }

            if (index < 0)
            {
                data = new CachedData();
                data.data = new byte[bufferSize];
                index = cache.Count;
                cache.Add(data);
                Debugger.LogError("----------------------------------------cached bytebuffer size->" + cache.Count);
            }

            data = cache[index];
            data.isUsing = true;
            buffer = data.data;

            return index;
        }
    }
}
