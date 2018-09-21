using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;

namespace GameBase
{
    internal class AndroidResourceLoader : ResourceLoader
    {
        public override byte[] SyncReadBytes(string path)
        {
            if (path == null)
                return null;
            byte[] data = null;

            bool doo = false;
            if (path.Length > 3)
            {
                if (path[0] == 'j' && path[1] == 'a' && path[2] == 'r')
                {
                    doo = true;
                }
            }

            if (doo)//streaming asset
            {
                AndroidAssetFileStream fs = new AndroidAssetFileStream();
                try
                {
                    bool v = fs.Open(path);
                    if (!v)
                        return null;

                    long len = fs.Length();
                    if (len <= 0)
                        return null;
                    data = new byte[len];

                    int re = fs.Read(data, 0, (int)len);
                    if (re <= 0)
                        return null;
                }
                catch (Exception e)
                {
                    Debugger.LogError("android resource loader sync read bytes failed 1->" + e.ToString());
                }

                fs.Close();
                return data;
            }
            else
            {
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
                    Debugger.LogError("sync read bytes exception->" + e.ToString());
                }

                return data;
            }
        }

        public override int SyncReadBytes(string path, int begin, int length, byte[] destBuf)
        {
            if (path == null)
                return -1;
            if (destBuf == null)
                return -2;
            if (destBuf.Length < length)
                return -103;

            bool doo = false;
            if (path.Length > 3)
            {
                if (path[0] == 'j' && path[1] == 'a' && path[2] == 'r')
                {
                    doo = true;
                }
            }

            if (doo)//streamming asset
            {
                AndroidAssetFileStream fs = new AndroidAssetFileStream();
                try
                {
                    bool v = fs.Open(path);
                    if (!v)
                        return -3;

                    long len = fs.Length();
                    if (len <= 0)
                        return -4;

                    if ((len - begin) < length)
                        return -5;

                    int re = fs.Read(destBuf, begin, length);
                    if (re <= 0)
                    {
                        Debugger.LogWarning("android resource loader sync read bytes re->" + re);
                        return -6;
                    }
                    fs.Close();
                    return (int)length;
                }
                catch (Exception e)
                {
                    Debugger.LogError("android resource loader sync read bytes failed 2->" + e.ToString());
                    fs.Close();
                    return -6;
                }
            }
            else
            {
                try
                {
                    using (FileStream fs = File.OpenRead(path))
                    {
                        if ((fs.Length - begin) < length)
                            return -7;

                        fs.Read(destBuf, begin, length);
                    }
                }
                catch (System.Exception e)
                {
                    Debugger.LogError("fragment sync read bytes exception->" + e.ToString());
                    return -4;
                }

                return length;
            }
        }

        public override IResourceFileStream LoadFile(string path)
        {
            if (path == null)
                return null;

            bool doo = false;
            if (path.Length > 3)
            {
                if (path[0] == 'j' && path[1] == 'a' && path[2] == 'r')
                {
                    doo = true;
                }
            }

            if (doo)
            {
                try
                {
                    AndroidAssetFileStream fs = new AndroidAssetFileStream();
                    bool v = fs.Open(path);
                    if (v)
                        return fs;
                    else
                        return null;
                }
                catch (Exception e)
                {
                    Debugger.LogError("android resource loader load file error->" + e.ToString());
                    return null;
                }
            }
            else
            {
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
                    Debugger.LogError("load file exception->" + e.ToString());
                    return null;
                }
            }
        }
    }
}
