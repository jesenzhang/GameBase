using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System;
using Example;


namespace GameBase
{
    public partial class ResUpdate : MonoBehaviour
    {
        /// <summary>
        /// Delegate UpdateBar
        /// </summary>
        /// <param name="curNum"> 当前已经下载的数量.</param>
        /// <param name="totalNum">总共要下载的数量</param>
        /// <param name="curSize">当前已经下载的大小</param>
        /// <param name="totalSize">当前需要下载的总大小</param>
        /// <param name="curFileSiz">当前下载的文件的大小</param>
        public delegate void UpdateBar(int curNum, int totalNum ,long curSize ,long totalSize, long curFileSize);

        private static string persistentDataPath = null;
        private static string streamingAssetsPath = null;

        void Awake()
        {
            inited = true;
            instance = this;
            DontDestroyOnLoad(gameObject);

            persistentDataPath = Application.persistentDataPath;
            streamingAssetsPath = Application.streamingAssetsPath;

            ZipConstants.DefaultCodePage = 65001;
            string path = "";
            if (Application.platform == RuntimePlatform.Android)
            {
                path = Application.persistentDataPath + "/MeData/";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.persistentDataPath + "/MeData/";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path = Application.dataPath + "/MeData/";
            }
            else if(Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = Application.dataPath.Replace("/Assets", "") + "/MeData/";
            }
            else if(Application.platform == RuntimePlatform.OSXEditor)
            {
                path = Application.dataPath.Replace("/s", "") + "/MeData/";
            }

            string temp = path.Remove(path.Length - 1);
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            localAssetPath = path;
            UnityEngine.Debug.Log("res path->" + path);
        }

        public static void DirectlyLoadInit(string root)
        {
            directlyLoadFiles = new CollectionFiles(root);
            directlyLoadFiles.Init();
        }

        public static void CheckClientVersion(string _remoteAssetPath, Action<float> onCheckUF, UpdateBar onUpdateBar, Action onUpdateOver)
        {
            if(Config.Detail_Debug_Log())
                Debug.Log("check client version 1");
            if (instance == null)
                return;

            if (Config.DirectlyLoadResource())
            {
                onCheckUF(1);
                onUpdateBar(1, 1, 1, 1, 1);
                onUpdateOver();
            }
            else
            {
                if(Config.Detail_Debug_Log())
                    Debug.Log("check client version 2");
                instance._CheckClientVersion(_remoteAssetPath, onCheckUF, onUpdateBar, onUpdateOver);
            }
        }

        private void _CheckClientVersion(string _remoteAssetPath, Action<float> onCheckUF, UpdateBar onUpdateBar, Action onUpdateOver)
        {
            if(Config.Detail_Debug_Log())
                Debug.Log("check client version 3->" + ResLoader.IsInited() + "^" + inited + "^" + over);
            if (!ResLoader.IsInited() || !inited || over)
                return;

            if(Config.Detail_Debug_Log())
                Debug.Log("check client version 4");
            remoteAssetPath = _remoteAssetPath;
            doUpdateUIOver = onUpdateOver;
            doUpdateBarV = onUpdateBar;
            doCheckUF = onCheckUF;

            InitVersionFile(() =>// check local version in dowload directory
            {
                if(Config.Detail_Debug_Log())
                    Debug.LogError("check client version 5");
                StartCoroutine(InitStreamingAssetVersionInfo(() => // check loacal version in  streamAsset directory
                {
                    if(Config.Detail_Debug_Log())
                        Debug.LogError("check client version 6");
                    BeginCheckVersion();//
                }));
            });
        }

        public static string GetAssetPath()
        {
            if (instance == null)
                return null;

            return instance.GetLocalAssetPath();
        }

        private string GetLocalAssetPath()
        {
            return localAssetPath;
        }

        public static void Clear()
        {
            if (instance == null)
                return;

            instance._Clear();
        }

        private void _Clear()
        {
            over = false;
            canUpdate = false;
            updateVersion = false;
            totalNum = 0;
            curNum = 0;
        }

        private void InitVersionFile(Action callback)
        {
            GameUtils.StringBuilderClear();
            GameUtils.stringBuilder.Append(GetLocalAssetPath());
            GameUtils.stringBuilder.Append(versionFileName);

            ResLoader.AsynReadBytesByPath(GameUtils.stringBuilder.ToString(), VersionFile.Type.DEFAULT, CB_EndReadVersionFile, callback, true);
        }

        private void CB_EndReadVersionFile(byte[] data, System.Object obj)
        {
            if (data != null)
            {
                GameUtils.MemoryStreamClear();
                GameUtils.ms.Write(data, 0, data.Length);
                GameUtils.ms.Position = 0;

                versionInfo = VersionInfo.Deserialize(GameUtils.ms);

                versionFileData = new VersionFileData();
                versionFileData.fileLocate = new List<byte>();

                nameToRelation.Clear();

                VersionInfoToData(versionInfo, (byte)FileLocate.None);

                if (obj != null)
                {
                    ((Action)obj)();
                }
            }
            else 
            {
                if (readVersionStamp == 0)
                {
                    ResLoader.AsynReadBytesByPath(Application.streamingAssetsPath + "/MeData/" + versionFileName, VersionFile.Type.DEFAULT, CB_EndReadVersionFile, obj);
                    readVersionStamp = 1;
                }
                else 
                {
                    if (obj != null)
                    {
                        ((Action)obj)();
                    }
                }
            }
        }

        private IEnumerator InitStreamingAssetVersionInfo(Action callback)
        {
            GameUtils.StringBuilderClear();
            GameUtils.stringBuilder.Append(Application.streamingAssetsPath);
            GameUtils.stringBuilder.Append("/MeData/");
            GameUtils.stringBuilder.Append(versionFileName);

            string path = GameUtils.stringBuilder.ToString();
            bool doo = false;
            if (path.Length > 3)
            {
                if (path[0] == 'j' && path[1] == 'a' && path[2] == 'r')
                {
                    doo = true;
                }
            }

            if (!doo)
                path = "file://" + path;

            WWW www = new WWW(path);
            yield return www;

            byte[] data = null;
            if (www.error == null)
                data = www.bytes;

            if (data != null)
            {
                ResLoader.RemoveImpurity(data, (arr) =>
                {
                    GameUtils.MemoryStreamClear();
                    GameUtils.ms.Write(data, 0, data.Length);
                    GameUtils.ms.Position = 0;

                    VersionInfo vi = VersionInfo.Deserialize(GameUtils.ms);
                    streamingAssetVersionInfo = vi;

                    if (www != null)
                        www.Dispose();

                    if (callback != null)
                    {
                        callback();
                    }
                });
            }
            else
            {
                if (www != null)
                    www.Dispose();

                if (callback != null)
                {
                    callback();
                }
            }
        }

        private void BeginCheckVersion()
        {
            if(Config.Detail_Debug_Log())
                Debug.Log("begin auto update init->" + remoteAssetPath);
            StartCoroutine(ResLoader.BeginDownload(remoteAssetPath + checkVersionFileName + GameUtils.GetSuffixOfURL(), CheckVersion, null, updatePBar));
        }

        private void VersionInfoToData(VersionInfo vi, byte fileLocate)
        {
            VersionFile vf;
            for (int i = 0, count = vi.Files.Count; i < count; i++)
            {
                vf = vi.Files[i];
                RelationData rd = new RelationData();
                rd.index = i;
                rd.origin = vf.Origin;
                rd.guid = vf.Guid;
                rd.encrypt = vf.Encrypt;
                rd.size = vf.Size;
                rd.fileType = vf.type;

                nameToRelation.Add(rd.origin, rd);

                versionFileData.fileLocate.Add(fileLocate);

                //childs
                for (int j = 0, jcount = vf.Childs.Count; j < jcount; j++)
                {
                    nameToRelation.Add(vf.Childs[j], rd);
                }
            }
        }

        private void CheckVersion(string url, byte[] data, System.Object obj)
        {
            if (data == null)
            {
                if (Config.Detail_Debug_Log())
                    Debug.LogError("download check version data failed");

                LuaInterface.LuaFunction func = LuaManager.GetFunction("OnDownloadCheckVersionDataFailed");
                if (func != null)
                    LuaManager.CallFunc_VX(func);
                else
                    Debug.LogError("script function 'OnDownloadCheckVersionDataFailed' is null");

                return;
            }

            GameUtils.MemoryStreamClear();
            GameUtils.ms.Write(data, 0, data.Length);
            GameUtils.ms.Position = 0;

            vfc = VersionForCheck.Deserialize(GameUtils.ms);
            updateVersion = false;
            if (Config.Detail_Debug_Log())
                Debugger.Log("versionInfo->" + (versionInfo == null) + "^" + vfc.Version);
            if (versionInfo != null)
            {
                Debugger.Log("vi->" + versionInfo.Version);
            }
            else if (streamingAssetVersionInfo != null)
            {
                versionInfo = streamingAssetVersionInfo;

                versionFileData = new VersionFileData();
                versionFileData.fileLocate = new List<byte>();

                nameToRelation.Clear();

                VersionInfoToData(versionInfo, (byte)FileLocate.StreamingAsset);
            }

            if (versionInfo == null || vfc.Version != versionInfo.Version)
            {
                Debugger.LogError("version not match, need update.");
                updateVersion = true;
                GameUtils.StringBuilderClear();
                GameUtils.stringBuilder.Append(remoteAssetPath);
                GameUtils.stringBuilder.Append(versionInfoFileName);
                GameUtils.stringBuilder.Append(GameUtils.GetSuffixOfURL());
                StartCoroutine(ResLoader.BeginDownload(GameUtils.stringBuilder.ToString(), DownloadVersionFile, null, updatePBar));
            }
            else
            {
                canUpdate = true;
                Debugger.Log("update nothing");
            }
        }

        private void DownloadVersionFile(string url, byte[] data, System.Object obj)
        {
            if (data == null)
            {
                if (Config.Detail_Debug_Log())
                    Debug.LogError("download version data failed");

                LuaInterface.LuaFunction func = LuaManager.GetFunction("OnDownloadVersionDataFailed");
                if (func != null)
                    LuaManager.CallFunc_VX(func);
                else
                    Debug.LogError("script function 'OnDownloadVersionDataFailed' is null");

                return;
            }

            if (Config.Detail_Debug_Log())
                Debug.Log("download version");

            GameUtils.BytesMD5Value(data, (md5Str) =>
            {
                if (Config.Detail_Debug_Log())
                    Debug.Log("download version 1->" + md5Str + "^" + vfc.Md5);
                if (md5Str != vfc.Md5.ToLower())
                {
                    Debugger.LogError("versionInfo file is invalid->" + md5Str + "^" + vfc.Md5);
                    return;
                }

                versionInfoData = new byte[data.Length];
                System.Array.Copy(data, versionInfoData, data.Length);
                ResLoader.RemoveImpurity(data, (arr) =>
                {
                    GameUtils.MemoryStreamClear();
                    GameUtils.ms.Write(data, 0, data.Length);
                    GameUtils.ms.Position = 0;

                    versionInfo = VersionInfo.Deserialize(GameUtils.ms);

                    if (Config.Detail_Debug_Log())
                        Debugger.Log("download version 2->" + versionInfo.Files.Count);

                    versionFileData = new VersionFileData();
                    versionFileData.fileLocate = new List<byte>();

                    nameToRelation.Clear();

                    VersionInfoToData(versionInfo, (byte)FileLocate.None);

                    totalNum = 0;

                    StartCoroutine(BeginCheckUpdateFiles());
                });
            });
        }

        private void AddCheckUpdateIndex(int gcount, List<int> updateList, List<long> sizeList)
        {
            {
                checkUpdateFileIndex++;
                if (checkUpdateFileIndex >= gcount)
                    BeginUpdate(updateList, sizeList);
            }
        }

        private void BeginUpdate(List<int> updateList, List<long> totalList)
        {
            canUpdate = true;

            totalSize = 0;
            for (int i = 0, count = totalList.Count; i < count; i++)
            {
                totalSize += totalList[i];
            }

            Debugger.LogError("download version->" + totalSize + "^" + updateList.Count + "^" + totalList.Count);
            curSize = 0;
            if (updateList.Count > 0)
            {
                if (totalSize > 0)
                {
                    totalNum = updateList.Count;
                    needUpdateList = updateList;

                    canUpdate = false;
                    LuaInterface.LuaFunction func = LuaManager.GetFunction("OnResourceNeedUpdate");
                    if (func != null)
                        LuaManager.CallFunc_VX(func, totalNum, totalSize);
                    else
                        Debug.LogError("script function 'OnResourceNeedUpdate' is null");
                }
#if UNITY_EDITOR
			else
				  Debugger.LogError("update files is invalid");
#endif
            }
        }

        private void _BeginResourceUpdate()
        {
            canUpdate = true;
            if (needUpdateList != null)
                UpdateNewVersion(needUpdateList);
            else
                Debug.LogError("begin update list is null");
        }

        public static void BeginResourceUpdate()
        {
            if (ResUpdate.instance != null)
                ResUpdate.instance._BeginResourceUpdate();
        }

        private IEnumerator BeginCheckUpdateFiles()
        {
            List<int> updateList = new List<int>();
            List<long> totalList = new List<long>();
            checkUpdateFileIndex = 0;
            yield return StartCoroutine(CheckUpdateFiles(updateList, totalList));
        }

        private void UpdateNewVersion(List<int> updateList)
        {
            int index;
            VersionFile vf;
            for (int i = 0, count = updateList.Count; i < count; i++)
            {
                index = updateList[i];
                GameUtils.StringBuilderClear();

                GameUtils.stringBuilder.Append(remoteAssetPath);
                vf = versionInfo.Files[index];
                GameUtils.stringBuilder.Append(vf.Guid);
                GameUtils.stringBuilder.Append(downloadFileExtension);
                GameUtils.stringBuilder.Append(GameUtils.GetSuffixOfURL());
                curFileSize = vf.Size;

                WwwWorkpool.instance.AddWork(GameUtils.stringBuilder.ToString(), -1, DownloadRes, index, false);
            }
        }

        private void DownloadRes(string url, string dataPath, System.Object obj)
        {
            try
            {
                if (obj == null)
                {
                    throw new System.Exception("download param is null");
                }

                int index = (int)obj;
                if (index < 0 || index >= versionInfo.Files.Count)
                {
                    throw new System.Exception("download error 1");
                }

                VersionFile vf = versionInfo.Files[index];

                string guid = vf.Guid;

                GameUtils.StringBuilderClear();

                GameUtils.stringBuilder.Append(GetLocalAssetPath());
                GameUtils.stringBuilder.Append(guid);

                string path = GameUtils.stringBuilder.ToString();
                {
                    string md5Str;

                    bool re = false;
                    using (FileStream fs = File.OpenRead(dataPath))
                    {
                        if (Config.Detail_Debug_Log())
                            Debugger.Log("res update down load res data->" + fs.Length);
                        re = UnZipData(fs, path, out md5Str);
                    }

                    File.Delete(dataPath);
#if UNITY_IPHONE
                    if (re)
                    {
                        if (Application.platform == RuntimePlatform.IPhonePlayer)
                            UnityEngine.iOS.Device.SetNoBackupFlag(GameUtils.stringBuilder.ToString());
                        //iPhone.SetNoBackupFlag(GameUtils.stringBuilder.ToString());
                    }
#endif

                    {
                        if (!re)
                        {
                            Debugger.LogError("data is null, reload");
                            WwwWorkpool.instance.AddWork(url, -1, DownloadRes, index, false);
                            return;
                        }

                        if (md5Str != vf.Md5)
                        {
                            WwwWorkpool.instance.AddWork(url, -1, DownloadRes, index, false);
                        }
                        else
                        {
                            curNum++;
                            curSize += vf.Size;
                            curFileSize = 0;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                //#if UNITY_EDITOR
                Debugger.LogError(e.ToString());
                //#endif
            }
        }

        private IEnumerator CheckUpdateFiles(List<int> updateList, List<long> totalSize)
        {
            for (int i = 0, count = versionInfo.Files.Count; i < count;)//++i)?
            {

                for (int j = 0; j < 20; j++)
                {
                    if (i == count - 1)
                        yield return StartCoroutine(CheckUF(updateList, versionInfo, i, totalSize));
                    else
                        StartCoroutine(CheckUF(updateList, versionInfo, i, totalSize));
                    i++;
                    if (i >= count)
                        yield break;
                }

                yield return StartCoroutine(CheckUF(updateList, versionInfo, i, totalSize));
                i++;
            }
        }

        private int VersionGuidFileIndex(VersionInfo versionInfo, string guid)
        {
            VersionFile vf;
            for (int i = 0, count = versionInfo.Files.Count; i < count; i++)
            {
                vf = versionInfo.Files[i];
                if (vf.Guid == guid)
                    return i;
            }

            return -1;
        }

        private IEnumerator CheckUF(List<int> updateList, VersionInfo versionInfo, int index, List<long> sizeList)
        {
            int gcount = versionInfo.Files.Count;
            if (doCheckUF != null)
            {
                doCheckUF((float)index / gcount);
            }

            GameUtils.StringBuilderClear();
            GameUtils.stringBuilder.Append(GetLocalAssetPath());

            VersionFile vf = versionInfo.Files[index];

            string guid = vf.Guid;
            GameUtils.stringBuilder.Append(guid);

            if (File.Exists(GameUtils.stringBuilder.ToString()))
            {
                versionFileData.fileLocate[index] = (byte)FileLocate.Download;
                AddCheckUpdateIndex(gcount, updateList, sizeList);
                yield break;
            }
            else
            {
                if (streamingAssetVersionInfo != null && (VersionGuidFileIndex(streamingAssetVersionInfo, guid) >= 0))
                {
                    versionFileData.fileLocate[index] = (byte)FileLocate.StreamingAsset;
                    AddCheckUpdateIndex(gcount, updateList, sizeList); ;
                    yield break;
                }

                versionFileData.fileLocate[index] = (byte)FileLocate.Download;
                sizeList.Add(vf.Size);
            }

            updateList.Add(index);

            AddCheckUpdateIndex(gcount, updateList, sizeList);
        }

        private void DeletePreviousFiles()
        {
            try
            {
                string ppath = null;

                ppath = GetLocalAssetPath();

                if (!Directory.Exists(ppath))
                    return;

                string[] files = Directory.GetFiles(ppath);
                short i, count, num = 0;
                string file;

                for (i = 0, count = (short)files.Length; i < count; i++)
                {
                    try
                    {
                        file = files[i];
                        file = Path.GetFileNameWithoutExtension(file);

                        if (file == Path.GetFileNameWithoutExtension(versionFileName))
                            continue;
                        if (GameUtils.IsUnDeleteFile(file))
                            continue;

                        int index = VersionGuidFileIndex(versionInfo, file);
                        if (index < 0)
                        {
                            num++;
                            File.Delete(files[i]);
                        }
                    }
                    catch
                    {
                    }
                }

#if UNITY_EDITOR
			  Debugger.Log("del files->" + num);
#endif
            }
            catch
            {

            }
        }

        private void SetFileLocate()
        {
            if (versionFileData == null)
                return;

            if (versionFileData.fileLocate[0] != (byte)FileLocate.None)
                return;

            if (streamingAssetVersionInfo == null)
            {
                for (int i = 0, count = versionFileData.fileLocate.Count; i < count; i++)
                    versionFileData.fileLocate[i] = (byte)FileLocate.Download;
            }
            else
            {
                for (int i = 0, count = versionInfo.Files.Count; i < count; i++)
                {
                    if (VersionGuidFileIndex(streamingAssetVersionInfo, versionInfo.Files[i].Guid) >= 0)
                    {
                        versionFileData.fileLocate[i] = (byte)FileLocate.StreamingAsset;
                    }

                    else
                        versionFileData.fileLocate[i] = (byte)FileLocate.Download;
                }
            }
        }

        private void DoOver()
        {
            Debugger.LogError(">>>>>>>>>>>>>>>>>>>>>>Update Over<<<<<<<<<<<<<<<<<<<<<<<<<<");

            if (doCheckUF != null)
            {
                doCheckUF(1f);
            }

            SetFileLocate();

            over = true;
            canUpdate = false;

            vfc = null;
            versionInfo = null;
            if (needUpdateList != null)
            {
                needUpdateList.Clear();
                needUpdateList = null;
            }

            streamingAssetVersionInfo = null;

            if (doUpdateUIOver != null)
            {
                doUpdateUIOver();
                curFileSize = 0;
            }

            if (reMs != null)
            {
                reMs.Dispose();
                reMs = null;
            }

            //load newer lua
            LuaLoader.GetInstance().Reset();

            MessagePool.CSSendMessage(null, MessagePool.UpdateResOver, Message.FilterTypeNothing, "");

            //test
            Dictionary<string, RelationData>.Enumerator e = nameToRelation.GetEnumerator();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            while (e.MoveNext())
            {
                sb.Append(e.Current.Key);
                sb.Append("\r\n");
            }

            Debug.Log("all version files->" + sb.ToString());
        }

        public static void ForceOver()
        {
            if (instance != null)
                instance._ForceOver();
        }

        private void _ForceOver()
        {
            canUpdate = true;
            updateVersion = false;
            totalNum = 1;
            curNum = 1;
        }

        void Update()
        {
            if (!canUpdate)
                return;
            if (totalNum > 0 && curNum <= totalNum)
            {
                if (doUpdateBarV != null)
                    doUpdateBarV(curNum, totalNum, curSize, totalSize, curFileSize);

                if (curNum == totalNum)
                    curNum++;
            }

            if (updateVersion)
            {
                if (versionInfo == null || versionInfo.Files == null)
                    return;
                if (curNum >= totalNum)
                {
                    GameUtils.MemoryStreamClear();

                    string path = GetLocalAssetPath() + versionFileName;
                    using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        fs.Write(versionInfoData, 0, versionInfoData.Length);
                        fs.Close();
                    }
#if UNITY_IPHONE
                    if (Application.platform == RuntimePlatform.IPhonePlayer)
                        UnityEngine.iOS.Device.SetNoBackupFlag(GameUtils.stringBuilder.ToString());
                    //iPhone.SetNoBackupFlag(path);
#endif

                    versionInfoData = null;
                    Debugger.LogError("save version file over");

                    DeletePreviousFiles();
                    DoOver();
                }
            }
            else
            {
                DoOver();
            }
        }

        //-------------------FUNC----------------------------------
        private static bool UnZipData(Stream s, string filePath, out string md5Str)
        {
            try
            {
                using (FileStream fs = File.Open(GameUtils.stringBuilder.ToString(), FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    using (ZipInputStream zipInputStream = new ZipInputStream(s))
                    {
                        ZipEntry zipEntry = zipInputStream.GetNextEntry();
                        int i = 2048;
                        byte[] unZipDataArr = new byte[i];
                        while (zipEntry != null)
                        {
                            if (!zipEntry.IsDirectory && zipEntry.Crc != 00000000L)
                            {
                                while (true)
                                {
                                    i = zipInputStream.Read(unZipDataArr, 0, unZipDataArr.Length);
                                    if (i > 0)
                                        fs.Write(unZipDataArr, 0, i);
                                    else
                                        break;
                                }
                            }

                            zipEntry = zipInputStream.GetNextEntry();
                        }

                        zipInputStream.Close();
                    }

                    long len = fs.Position;
                    fs.Position = 0;
                    md5Str = GameUtils.SyncStreamMd5Value(fs);
                    fs.Position = len;
                    fs.Flush();
                    fs.Close();
                }
            }
            catch (System.Exception e)
            {
                md5Str = null;
                Debugger.LogError(e.ToString());
                return false;
            }

            return true;
        }

        private static bool UnZipData(byte[] data, string filePath, out string md5Str)
        {
            MemoryStream ms = new MemoryStream(data);
            ms.Position = 0;

            UnZipData(ms, filePath, out md5Str);

            ms.Dispose();

            return true;
        }

        public static bool VersionHasResource(string origin)
        {
            if (instance == null)
                return false;
            return instance._VersionHasResource(origin);
        }

        private bool _VersionHasResource(string origin)
        {
            if (Config.DirectlyLoadResource())
            {
                if (directlyLoadFiles == null)
                    return false;

                string destName;
                string path;
                bool encrypt;
                VersionFile.Type fileType;
                directlyLoadFiles.GetRealPath(origin, out destName, out path, out encrypt, out fileType);
                if (path != null)
                    return true;
                else
                    return false;
            }
            else
            {
                if (origin == null)
                    return false;

                RelationData rd;
                if (!nameToRelation.TryGetValue(origin, out rd))
                {
                    Debugger.LogWarning("file not exist in version->" + origin);
                    return false;
                }

                if (rd != null)
                    return true;
                else
                    return false;
            }
        }

        public static void GetLoadDetails(string origin, out string destName, out string path, out int size, out bool encrypt, out VersionFile.Type fileType)
        {
            if (instance == null)
            {
                destName = null;
                path = null;
                size = -1;
                encrypt = false;
                fileType = VersionFile.Type.DEFAULT;
                return;
            }

            instance._GetLoadDetails(origin, out destName, out path, out size, out encrypt, out fileType);
        }

        private void _GetLoadDetails(string origin, out string destName, out string path, out int size, out bool encrypt, out VersionFile.Type fileType)
        {
            destName = null;
            path = null;
            size = -1;
            encrypt = false;
            fileType = VersionFile.Type.DEFAULT;

            if (Config.DirectlyLoadResource())
            {
                if (directlyLoadFiles == null)
                    return;

                directlyLoadFiles.GetRealPath(origin, out destName, out path, out encrypt, out fileType);
            }
            else
            {
                if (origin == null)
                    return;

                RelationData rd;
                if (!nameToRelation.TryGetValue(origin, out rd))
                {
                    Debugger.LogWarning("file not exist in version->" + origin);
                    return;
                }

                if (rd != null)
                {
                    lock (lock_getpath)
                    {
                        GameUtils.StringBuilderClear();
                        int fl = versionFileData.fileLocate[rd.index];
                        if (fl == (int)FileLocate.Download)
                        {
                            GameUtils.stringBuilder.Append(GetLocalAssetPath());
                        }
                        else
                        {
                            GameUtils.stringBuilder.Append(streamingAssetsPath);
                            GameUtils.stringBuilder.Append("/MeData/");
                        }

                        GameUtils.stringBuilder.Append(rd.guid);
                        path = GameUtils.stringBuilder.ToString();
                    }
                    destName = rd.origin;
                    size = rd.size;
                    encrypt = rd.encrypt;
                    fileType = rd.fileType;
                }
                //#if UNITY_EDITOR
                else
                    Debugger.LogError("this path is not in versionInfo->" + origin);
                //#endif //UNITY_EDITOR
            }
        }
        
        //#if UNITY_EDITOR

        internal static string ConvertPathToName(string path)
        {
            if (Config.DirectlyLoadResource())
            {
                return path;
            }
            else
            {
                if (instance != null)
                    return instance._ConvertPathToName(path);
                return null;
            }
        }

        private string _ConvertPathToName(string path)
        {
            if (path == null || path == "")
                return null;
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (versionInfo == null)
                return null;
            int index = VersionGuidFileIndex(versionInfo, fileName);
            if (index < 0)
            {
                Debugger.LogWarning("convert path to name failed:not exist this path->" + fileName);
                return null;
            }

            Dictionary<string, RelationData>.Enumerator e = nameToRelation.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.index == index)
                    return e.Current.Key;
            }

            return null;
        }

        #region Read Resources Before Update

        public static void EarlyInitVersionFile(Action callback)
        {
            if (instance == null)
                return;
            if (Config.DirectlyLoadResource())
                callback();
            else
                instance._EarlyInitVersionFile(callback);
        }

        private void _EarlyInitVersionFile(Action callback)
        {
            GameUtils.StringBuilderClear();
            GameUtils.stringBuilder.Append(GetLocalAssetPath());
            GameUtils.stringBuilder.Append(versionFileName);

            if (Config.Detail_Debug_Log())
                Debugger.Log("early init->" + GameUtils.stringBuilder.ToString());

            ResLoader.AsynReadBytesByPath(GameUtils.stringBuilder.ToString(), VersionFile.Type.DEFAULT, Early_CB_EndReadVersionFile, callback, true);
        }

        public static void ClearEarlyInfo()
        {
            if (instance == null)
                return;
            instance._ClearEarlyInfo();
        }

        private void _ClearEarlyInfo()
        {
            nameToRelation.Clear();
            versionInfo = null;
        }

        private void Early_CB_EndReadVersionFile(byte[] data, System.Object obj)
        {
            if (Config.Detail_Debug_Log())
                Debugger.Log("read version file->" + (data == null));
            if (data != null)
            {
                GameUtils.MemoryStreamClear();
                GameUtils.ms.Write(data, 0, data.Length);
                GameUtils.ms.Position = 0;

                versionInfo = VersionInfo.Deserialize(GameUtils.ms);
            }

            EarlyInitStreamingAssetVersion((Action)obj);
        }

        private void EarlyInitStreamingAssetVersion(Action callback)
        {
            GameUtils.StringBuilderClear();
            GameUtils.stringBuilder.Append(Application.streamingAssetsPath);
            GameUtils.stringBuilder.Append("/MeData/");
            GameUtils.stringBuilder.Append(versionFileName);

            if (Config.Detail_Debug_Log())
                Debugger.Log("early init streamingasset->" + GameUtils.stringBuilder.ToString());
            string path = GameUtils.stringBuilder.ToString();

            VersionInfo earlyStreamingAssetVersionInfo = null;
            byte[] data = ResLoader.SyncReadBytesByPath(path);
            if (data != null)
            {
                GameUtils.MemoryStreamClear();
                GameUtils.ms.Write(data, 0, data.Length);
                GameUtils.ms.Position = 0;

                earlyStreamingAssetVersionInfo = VersionInfo.Deserialize(GameUtils.ms);
            }

            EarlyInitVersion(earlyStreamingAssetVersionInfo, callback);
        }

        private void EarlyInitVersion(VersionInfo earlyStreamingAssetVersionInfo, Action callback)
        {
            if (Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("early init process->" + (versionInfo == null) + "^" + (earlyStreamingAssetVersionInfo == null));

            if (versionInfo == null)
            {
                if (earlyStreamingAssetVersionInfo != null && earlyStreamingAssetVersionInfo.Files != null && earlyStreamingAssetVersionInfo.Files.Count > 0)
                {
                    versionFileData = new VersionFileData();
                    versionFileData.fileLocate = new List<byte>();
                    nameToRelation.Clear();

                    VersionInfoToData(earlyStreamingAssetVersionInfo, (byte)FileLocate.StreamingAsset);

                    versionInfo = earlyStreamingAssetVersionInfo;
                }
            }
            else
            {
                if (versionInfo.Files != null && versionInfo.Files.Count > 0)
                {
                    versionFileData = new VersionFileData();
                    versionFileData.fileLocate = new List<byte>();

                    nameToRelation.Clear();

                    VersionInfoToData(versionInfo, (byte)FileLocate.None);

                    Dictionary<string, byte> streamDic = new Dictionary<string, byte>();
                    for (int i = 0, max = earlyStreamingAssetVersionInfo.Files.Count; i < max; i++)
                    {
                        streamDic.Add(earlyStreamingAssetVersionInfo.Files[i].Guid, 0);
                    }

                    if (Config.Detail_Debug_Log())
                        Debugger.Log("early init version info->" + (versionInfo == null) + "^" + (earlyStreamingAssetVersionInfo == null));
                    if (streamDic.Count > 0)
                    {
                        for (int i = 0, max = versionInfo.Files.Count; i < max; i++)
                        {
                            string guid = versionInfo.Files[i].Guid;
                            if (streamDic.ContainsKey(guid))
                            {
                                versionFileData.fileLocate[i] = (byte)FileLocate.StreamingAsset;
                            }
                            else
                            {
                                versionFileData.fileLocate[i] = (byte)FileLocate.Download;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0, max = versionInfo.Files.Count; i < max; i++)
                        {
                            versionFileData.fileLocate[i] = ((byte)FileLocate.Download);
                        }
                    }
                }
            }

            Debugger.Log("early init over");
            if (callback != null)
            {
                callback();
            }
        }

        #endregion
    }
}
