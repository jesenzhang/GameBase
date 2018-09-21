using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Debug = UnityEngine.Debug;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using Example;

namespace GameBase
{
    public partial class ResLoader : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_EDITOR
        [DllImport("Common")]
#else
		[DllImport("__Internal")]
#endif
        private static extern void DataDecode(byte[] arr, int len);

        private ResourceLoader platformLoader = null;

        public delegate void EndReadBytes(byte[] data, System.Object obj);


        void Awake()
        {
            if (instance != null)
                return;

            instance = this;

            if (Application.platform == RuntimePlatform.Android)
            {
                platformLoader = new AndroidResourceLoader();
            }
            else
            {
                platformLoader = new DefaultResourceLoader();
            }
        }

        public static bool IsInited()
        {
            return instance != null;
        }

        internal static void RemoveImpurity(byte[] bytes, Action<byte[]> call, bool async = true)
        {
            if (bytes == null)
            {
                if (call != null)
                    call(null);
                return;
            }

            _RemoveImpurity(bytes, call, async);
        }

        public static UnityEngine.Object SearchAsset(string path)
        {
            if (instance == null || instance.platformLoader == null)
                return null;

            return instance.platformLoader.Search(path);
        }

        public static void Unload(UnityEngine.Object asset)
        {
            if (instance == null || instance.platformLoader == null)
                return;

            instance.platformLoader.UnloadAsset(asset);
        }

        private static void _RemoveImpurity(byte[] bytes, Action<byte[]> call, bool async = true)
        {
            if (async && call == null)
                return;
            if (bytes == null)
            {
                if (call != null)
                    call(null);
                return;
            }

            if (async)
            {
                ThreadTask.RunAsync(() =>
                {
                    DataDecode(bytes, bytes.Length);
                },
                ()=> 
                {
                    call(bytes);
                });
            }
            else
                DataDecode(bytes, bytes.Length);
        }

        public static bool RemoveAssetCacheByName(string name, bool immediately = false, bool force = false)
        {
            if (instance == null || instance.platformLoader == null)
                return false;
            return instance.platformLoader.RemoveCache(name, immediately, force);
        }

        private static bool RemoveAssetCache(string name, bool immediately = false, bool force = false)
        {
            if (instance == null || instance.platformLoader == null)
                return false;

            return instance.platformLoader.RemoveCache(name, immediately, force);
        }

        public static byte[] SyncReadBytesByName(string name)
        {
            if (instance == null || instance.platformLoader == null)
                return null;
            string destName;
            string path;
            int size;
            bool encrypt;
            VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null)
                return null;
            return __SyncReadBytesByPath(name, destName, path, fileType, encrypt);
        }

        public static byte[] SyncReadBytesByPath(string path, bool removeImpurity = true)
        {
            return __SyncReadBytesByPath(null, null, path, VersionFile.Type.DEFAULT, removeImpurity);
        }

        private static byte[] __SyncReadBytesByPath(string originName, string destName, string path, VersionFile.Type fileType, bool removeImpurity = true)
        {
            if (instance == null || instance.platformLoader == null)
                return null;

            byte[] data = null;
            if (fileType == VersionFile.Type.COMBINE_FILE)
            {
                CombineFile cf = CombineFileManager.GetInstance().GetCombineFile(destName);
                data = cf.Read(originName);
            }
            else
            {
                data = instance.platformLoader.SyncReadBytes(path);
            }
            
            if (data != null)
            {
                if (removeImpurity)
                    DataDecode(data, data.Length);
            }

            return data;
        }

        public static byte[] SyncDownloadBytes(string path, bool removeImpurity = true)
        {
            byte[] rawBytes = null;
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                www.Send();
                while (true)
                {
                    if(www.isNetworkError || www.isHttpError)
                    {
                        Debugger.LogError("failed to load path->" + path);
                        break;
                    }

                    if (www.isDone)
                        break;
                }

                try
                {
                    if (www.isDone && (!www.isNetworkError && !www.isHttpError))
                    {
                        rawBytes = www.downloadHandler.data;
                        if (removeImpurity)
                            _RemoveImpurity(rawBytes, null, false);

                        return rawBytes;
                    }
                    else
                    {
                        Debug.LogError("sync read file error->" + www.error + "^" + ResUpdate.ConvertPathToName(path));
                    }
                }
                catch (Exception e)
                {
                    Debugger.LogError("syn read bytes failed->" + e.ToString());
                }
            }

            return null;
        }

        internal static void LoadByPath(string originName, string destName, string path, Example.VersionFile.Type fileType, int totalSize, ResourceLoader.EndLoadBundle endLoad, System.Object obj, bool assetBundle = false)
        {
            if (instance == null || instance.platformLoader == null)
                return;
            instance.platformLoader.LoadBundle(originName, destName, path, fileType, endLoad, obj, assetBundle);
        }

        public static void LoadByName(string name, ResourceLoader.EndLoadBundle endLoad, System.Object obj, bool assetBundle = false)
        {
            if (instance == null || instance.platformLoader == null)
                return;

            string destName;
            string path;
            int size;
            bool encrypt;
            Example.VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null)
            {
                Debugger.LogWarning("load name is not exist in version->" + name);
                if (endLoad != null)
                    endLoad(null, obj);
                return;
            }

            LoadByPath(name, destName, path, fileType, size, endLoad, obj, assetBundle);
        }

        public static void AsynReadBytesByName(string name, EndReadBytes endRead, System.Object obj, bool cb_whatever = false)
        {
            if (instance == null || instance.platformLoader == null)
                return;

            instance._AsynReadBytesByName(name, endRead, obj, cb_whatever);
        }

        private void _AsynReadBytesByName(string name, EndReadBytes endRead, System.Object obj, bool cb_whatever = false)
        {
            if (name == null || name == "")
                return;
            string destName;
            string path;
            int size;
            bool encrypt;
            VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null)
            {
                if (cb_whatever)
                {
                    if (endRead != null)
                        endRead(null, obj);
                }
                return;
            }

            __AsynReadBytesByPath(name, destName, path, fileType, endRead, obj, cb_whatever, encrypt);
        }

        public static void AsynReadBytesByPath(string path, VersionFile.Type fileType, EndReadBytes endRead, System.Object obj, bool cb_whatever = false, bool needRemoveImpurity = true)
        {
            __AsynReadBytesByPath(null, null, path, VersionFile.Type.DEFAULT, endRead, obj, cb_whatever, needRemoveImpurity);
        }

        private static void __AsynReadBytesByPath(string originName, string destName, string path, VersionFile.Type fileType, EndReadBytes endRead, System.Object obj, bool cb_whatever = false, bool needRemoveImpurity = true)
        {
            if (instance == null || instance.platformLoader == null)
                return;

            byte[] arr = null;
            if (fileType == VersionFile.Type.COMBINE_FILE)
            {
                CombineFile cf = CombineFileManager.GetInstance().GetCombineFile(destName);
                arr = cf.Read(originName);
            }
            else
            {
                arr = instance.platformLoader.SyncReadBytes(path);
            }

            if (arr != null)
            {
                if (needRemoveImpurity)
                {
                    ThreadTask.RunAsync(() =>
                    {
                        DataDecode(arr, arr.Length);
                    },
                    ()=> 
                    {
                        if (endRead != null)
                            endRead(arr, obj);
                    });
                }
                else
                {
                    if (endRead != null)
                        endRead(arr, obj);
                }
            }
            else
            {
                if (cb_whatever)
                {
                    if (endRead != null)
                        endRead(arr, obj);
                }
            }
        }

        public static void AsynReadUTF8ByName(string name, Action<string, System.Object> endRead, System.Object obj, bool cb_whatever = false)
        {
            if (instance == null)
                return;

            instance._AsynReadUTF8ByName(name, endRead, obj, cb_whatever);
        }

        private void _AsynReadUTF8ByName(string name, Action<string, System.Object> endRead, System.Object obj, bool cb_whatever = false)
        {
            if (name == null || name == "")
                return;
            string destName;
            string path;
            int size;
            bool encrypt;
            VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null)
            {
                if (cb_whatever)
                {
                    if (endRead != null)
                        endRead(null, obj);
                }
                return;
            }

            __AsynReadBytesByPath(name, destName, path, fileType, (bytes, @object) =>
            {
                if (bytes != null)
                {
                    string temp = System.Text.Encoding.UTF8.GetString(bytes);
                    if (endRead != null)
                    {
                        endRead(temp, @object);
                    }
                }
            }, obj, cb_whatever, encrypt);
        }

        public static IResourceFileStream LoadFileStreamByPath(string path)
        {
            if (instance == null || instance.platformLoader == null)
                return null;

            return instance._LoadFileStreamByPath(path);
        }

        public static IResourceFileStream LoadFileStreamByName(string name)
        {
            if (instance == null || instance.platformLoader == null)
                return null;

            return instance._LoadFileStreamByName(name);
        }

        private IResourceFileStream _LoadFileStreamByName(string name)
        {
            string destName;
            string path;
            int size;
            bool encrypt;
            VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);

            if (path == null)
                return null;

            return LoadFileStreamByPath(path);
        }

        private IResourceFileStream _LoadFileStreamByPath(string path)
        {
            return platformLoader.LoadFile(path);
        }

        private IEnumerator DoDownload(string path, EndDownload endDownload, System.Object ud, UpdatePBar upb, int index)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                if (upb != null && updatePBarDic.ContainsKey(www))
                    updatePBarDic.Remove(www);
                updatePBarDic.Add(www, upb);

                yield return www.Send();

                if (upb != null)
                {
                    upb(1);
                }

                updatePBarDic.Remove(www);

                if ((www.isNetworkError || www.isHttpError) && www.error.Length > 0)
                {
                    Debugger.LogError("Failed to download res: " + path + " Error: " + www.error);
                    www.Dispose();
                    if (index < 5)
                        StartCoroutine(instance._BeginDownload(path, endDownload, ud, upb, index++));
                    else
                        endDownload(www.url, null, ud);
                    yield break;
                }

                endDownload(www.url, www.downloadHandler.data, ud);
            }
        }

        public static IEnumerator BeginDownload(string path, EndDownload endDownload, System.Object ud, UpdatePBar upb)
        {
            if (instance == null)
                yield break;

            yield return instance.StartCoroutine(instance._BeginDownload(path, endDownload, ud, upb, 0));
        }

        private IEnumerator _BeginDownload(string path, EndDownload endDownload, System.Object ud, UpdatePBar upb, int index)
        {
            if (path == null || path == "")
                yield break;
            yield return StartCoroutine(DoDownload(path, endDownload, ud, upb, index));
        }

        public static void HelpLoadAsset(AssetBundle ab, string name, HelpLoadCallback hlcb, System.Object param, Type type)
        {
            if (instance == null)
                return;

            instance._HelpLoadAsset(ab, name, hlcb, param, type);
        }

        private void _HelpLoadAsset(AssetBundle ab, string name, HelpLoadCallback hlcb, System.Object param, Type type)
        {
            if (ab == null || hlcb == null)
                return;
            StartCoroutine(HelpLoad(ab, name, hlcb, param, type));
        }

        private IEnumerator HelpLoad(AssetBundle ab, string name, HelpLoadCallback hlcb, System.Object param, Type type)
        {
            AssetBundleRequest abr = ab.LoadAssetAsync(name, type);
            yield return abr;

            if (hlcb != null)
            {
                hlcb(abr, param);
            }
        }

        void LateUpdate()
        {
            if (updatePBarDic.Count > 0)
            {
                Dictionary<UnityWebRequest, UpdatePBar>.Enumerator e = updatePBarDic.GetEnumerator();
                while (e.MoveNext())
                {
                    if (e.Current.Value != null)
                    {
                        e.Current.Value(e.Current.Key.downloadProgress);
                    }
                }
            }

            if (platformLoader != null)
                platformLoader.Update();
        }
    }
}