using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace GameBase
{
    public static class LoadAndToTemp
    {
        public delegate void EndLoadToTemp(string tempPath);

        class TempData
        {
            public string path;
            public long time;
        }

        private static Dictionary<string, TempData> tempDic = new Dictionary<string, TempData>();

        private static string tempPath = null;

#if UNITY_ANDROID || UNITY_EDITOR
    [DllImport("Common")]
#else
        [DllImport("__Internal")]
#endif
        private static extern void DataDecodeFrag(byte[] src, int srclen, int totalLen, ref int pindex);

        private static CachedByteArray cachedBuffer = new CachedByteArray(1024 * 1024 * 2);

        private static object lockBuffer = new object();
        private static object lockTemp = new object();



        private static void _RemoveImpurityFrag(byte[] src, int datalen, int totalLen, ref int pindex)
        {
            DataDecodeFrag(src, datalen, totalLen, ref pindex);
        }

        private static void ClearTemp()
        {
            long cur = SDateTime.TotalMilliseconds;
            lock (lockTemp)
            {
                Dictionary<string, TempData>.Enumerator e = tempDic.GetEnumerator();
                List<string> list = new List<string>();
                List<string> keyList = new List<string>();
                while (e.MoveNext())
                {
                    if ((cur - e.Current.Value.time) > 60000)
                    {
                        keyList.Add(e.Current.Key);
                        list.Add(e.Current.Value.path);
                    }
                }

                if (list.Count > 0)
                {
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        tempDic.Remove(keyList[i]);
                        File.Delete(list[i]);
                    }
                }
            }
        }

        private static void AddToTempDic(string key, TempData td)
        {
            lock (lockTemp)
            {
                tempDic.Add(key, td);
            }
        }

        private static void RemoveFromTempDic(string key)
        {
            lock (lockTemp)
            {
                tempDic.Remove(key);
            }
        }

        private static bool TryGetValueTempDic(string key, out TempData data)
        {
            lock (lockTemp)
            {
                if (tempDic.TryGetValue(key, out data))
                    return true;

                data = null;
                return false;
            }
        }

        public static string GenTempPath()
        {
            return GenerateTempPath();
        }

        private static string GenerateTempPath()
        {
            if (tempPath == null)
            {
                string path = ResUpdate.GetAssetPath();
                tempPath = path + "temp/";

                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);
                }

                Directory.CreateDirectory(tempPath);
            }

            return tempPath + System.Guid.NewGuid().ToString();
        }

        public static void LoadToTemp(string path, bool removeImpurity, EndLoadToTemp endcall)
        {
            string rePath = null;
            ThreadTask.RunAsync(() =>
            {
                try
                {
                    using (FileStream fs = File.OpenRead(path))
                    {
                        byte[] buffer;
                        int bufferID;
                        lock (lockBuffer)
                        {
                            bufferID = cachedBuffer.GetByteBuffer(out buffer);
                        }

                        if (bufferID >= 0)
                        {
                            FileStream tfs = LoadToTempAdditive_Begin(path, out rePath);
                            try
                            {
                                int bufferLen = buffer.Length;
                                int remain = (int)fs.Length;
                                int total = remain;
                                int pindex = 0;
                                int readlen = 0;
                                int remainTime = 0;
                                while (remain > 0)
                                {
                                    remainTime++;
                                    if (remainTime > 1000)
                                    {
                                        Debugger.LogError("error read file temp large");
                                        rePath = null;
                                        break;
                                    }
                                    if (remain > bufferLen)
                                        readlen = bufferLen;
                                    else
                                        readlen = remain;

                                    if (readlen > 0)
                                    {
                                        fs.Read(buffer, 0, readlen);
                                        LoadToTempAdditive(rePath, tfs, buffer, readlen, total, removeImpurity, ref pindex);
                                    }

                                    remain -= readlen;
                                    if (remain <= 0)
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Debugger.LogError("error read file temp->" + e.ToString());
                                rePath = null;
                            }

                            lock (lockBuffer)
                            {
                                cachedBuffer.RecycleByteBuffer(bufferID);
                            }
                            LoadToTempAdditive_End(rePath, tfs);
                        }
                        else
                        {
                            Debugger.LogError("load to temp get cached buffer failed->" + bufferID);
                        }

                        fs.Close();
                    }
                }
                catch (System.Exception e)
                {
                    Debugger.LogError("read file exception->" + path + "^" + e.ToString());
                }
            },
            ()=> 
            {
                if (endcall != null)
                    endcall(rePath);
            });
        }

        public static int CheckLoadTemp(string path, out string tempPath)
        {
            int re = -1;
            tempPath = null;

            TempData td;

            if (TryGetValueTempDic(path, out td))
            {
                if (td != null)
                {
                    td.time = SDateTime.TotalMilliseconds;
                    tempPath = td.path;
                    re = 0;
                }
                else
                {
                    RemoveFromTempDic(path);
                    UnityEngine.Debug.LogError("missing path temp path->" + path);
                }
            }

            ClearTemp();

            return re;
        }

        public static FileStream LoadToTempAdditive_Begin(string path, out string tempPath)
        {
            string tp;
            tp = GenerateTempPath();
            TempData td = new TempData();
            td.path = tp;
            td.time = SDateTime.TotalMilliseconds;
            AddToTempDic(path, td);
            tempPath = tp;
            return File.OpenWrite(tp);
        }

        public static void LoadToTempAdditive(string path, FileStream fs, byte[] data, int datalen, int totalLen, bool removeImpurity, ref int pindex)
        {
            if(removeImpurity)
                _RemoveImpurityFrag(data, datalen, totalLen, ref pindex);
            fs.Write(data, 0, datalen);
        }

        public static void LoadToTempAdditive_End(string path, FileStream fs)
        {
            fs.Flush();
            fs.Close();
        }

        public static void RemovePath(string path)
        {
            RemoveFromTempDic(path);
        }

        public static void LoadToTemp(byte[] data, string path, EndLoadToTemp endcall, bool removeImpurity)
        {
            TempData td;
            if(TryGetValueTempDic(path, out td))
            {
                if (td != null)
                {
                    td.time = SDateTime.TotalMilliseconds;
                    endcall(td.path);
                    return;
                }
                else
                {
                    RemoveFromTempDic(path);
                    UnityEngine.Debug.LogError("missing path temp path->" + path);
                }
            }

            ClearTemp();

            if (removeImpurity)
            {
                ResLoader.RemoveImpurity(data, (tdata) =>
                {
                    if(TryGetValueTempDic(path, out td))
                    {
                        if (td != null)
                            endcall(td.path);
                        else
                            UnityEngine.Debug.LogError("temp dic contains key, but value is null->" + path);
                    }
                    else
                    {
                        string tp = GenerateTempPath();
                        td = new TempData();
                        td.path = tp;
                        td.time = SDateTime.TotalMilliseconds;
                        AddToTempDic(path, td);
                        using (FileStream f = File.OpenWrite(tp))
                        {
                            f.Write(tdata, 0, tdata.Length);
                            f.Flush();
                            f.Close();
                        }

                        endcall(tp);
                    }
                });
            }
            else
            {
                if(TryGetValueTempDic(path, out td))
                {
                    if (td != null)
                        endcall(td.path);
                    else
                        UnityEngine.Debug.LogError("temp dic contains key, but value is null->" + path);
                }
                else
                {
                    string tp = GenerateTempPath();
                    td = new TempData();
                    td.path = tp;
                    td.time = SDateTime.TotalMilliseconds;
                    AddToTempDic(path, td);
                    using (FileStream f = File.OpenWrite(tp))
                    {
                        f.Write(data, 0, data.Length);
                        f.Flush();
                        f.Close();
                    }

                    endcall(tp);
                }
            }
        }
    }
}
