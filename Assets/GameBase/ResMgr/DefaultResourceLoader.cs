
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameBase
{
    internal class DefaultResourceLoader : ResourceLoader
    {
        public override byte[] SyncReadBytes(string path)
        {
            byte[] data = null;
            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    if (fs.Length > 0)
                    {
                        data = new byte[fs.Length];
                        fs.Read(data, 0, (int)fs.Length);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("sync read bytes exception->" + e.ToString());
            }

            return data;
        }

        public override int SyncReadBytes(string path, int begin, int length, byte[] destBuf)
        {
            if (destBuf == null)
                return -1;
            if (destBuf.Length < length)
                return -2;

            try
            {
                using (FileStream fs = File.OpenRead(path))
                {
                    if ((fs.Length - begin) < length)
                        return -3;

                    fs.Read(destBuf, begin, length);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("fragment sync read bytes exception->" + e.ToString());
                return -4;
            }

            return length;
        }

        public override IResourceFileStream LoadFile(string path)
        {
            if (path == null)
                return null;
            try
            {
                DefaultAssetFileStream fs = new DefaultAssetFileStream();
                bool v = fs.Open(path);
                if (v)
                    return fs;
                else
                    return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError("load file exception->" + e.ToString());
                return null;
            }
        }
    }
}
