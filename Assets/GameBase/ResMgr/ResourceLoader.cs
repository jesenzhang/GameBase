using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace GameBase
{
    public class ResourceLoader : IPlatformResourceLoader
    {
        public delegate void EndLoadBundle(UnityEngine.Object asset, System.Object param);

        private Dictionary<string, Request> cache = new Dictionary<string, Request>();

        private class Request
        {
            public UnityEngine.Object asset;
            public List<System.Object> ops = null;
            public List<System.Object> uds = null;
            public int referenceNum;
            public RelationAssetFile raf = null;
        }

        class Removing
        {
            public Request request;
            public string name;
        }

        private List<Removing> needUnLoad = new List<Removing>();

        public virtual byte[] SyncReadBytes(string path) { throw new System.NotImplementedException(); }
        public virtual int SyncReadBytes(string path, int begin, int length, byte[] destBuf){ throw new System.NotImplementedException(); }
        public virtual IResourceFileStream LoadFile(string path) { throw new System.NotImplementedException(); }

        private void PushOP(Request request, System.Object op, System.Object ud)
        {
            if (request.ops == null)
            {
                request.ops = new List<System.Object>();
                request.uds = new List<System.Object>();
            }

            request.ops.Add(op);
            request.uds.Add(ud);
        }

        private void ProcessRequest(Request request, System.Object op, System.Object ud)
        {
            UnityEngine.Object asset = null;
            if (request.raf != null)
                asset = request.raf.GetMainAsset();
            else
                asset = request.asset;

            if (asset != null)
            {
                if (op != null)
                {
                    try
                    {
                        ((EndLoadBundle)op)(asset, ud);
                    }
                    catch (System.Exception e)
                    {
                        Debugger.LogError("process res endload func error->" + e.ToString());
                    }
                }

                if (request.ops != null)
                {
                    for (int i = 0; i < request.ops.Count; i++)
                    {
                        if (request.ops[i] != null)
                        {
                            try
                            {
                                ((EndLoadBundle)request.ops[i])(asset, request.uds[i]);
                            }
                            catch (System.Exception e)
                            {
                                Debugger.LogError("process res endload func error 1->" + e.ToString());
                            }
                        }
                    }

                    request.ops = null;
                    request.uds = null;
                }
            }
        }

        public void LoadBundle(string originName, string destName, string path, Example.VersionFile.Type fileType, EndLoadBundle endLoad, System.Object endLoadParam, bool assetBundle = false)//, bool cb_whatever = false)
        {
            if (path == null || path == "")
                return;

            Request request;
            if (!cache.TryGetValue(originName, out request))
            {
                request = new Request();
                cache.Add(originName, request);

                if (fileType == Example.VersionFile.Type.RELATION_FILE)
                {
                    CoroutineHelper.CreateCoroutineHelper(CreateAssetBundle_RelationFile(originName, destName, request, endLoad, endLoadParam));
                }
                else
                    CreateAssetBundle(originName, destName, path, fileType, request, endLoad, endLoadParam, assetBundle);
            }
            else
            {
                if (request.referenceNum < 0)
                {
                    if(Config.Debug_Log())
                        Debugger.LogError("referencenum is invalid->" + request.referenceNum);
                    request.referenceNum = 0;
                }
                request.referenceNum++;

                if (fileType == Example.VersionFile.Type.RELATION_FILE)
                {
                    if (request.raf.GetMainAsset() == null)
                    {
                        PushOP(request, endLoad, endLoadParam);
                    }
                    else
                    {
                        if (endLoad != null)
                            endLoad(request.raf.GetMainAsset(), endLoadParam);
                    }
                }
                else
                {
                    if (request.asset == null)
                    {
                        PushOP(request, (System.Object)endLoad, endLoadParam);
                    }
                    else
                    {
                        if (endLoad != null)
                            endLoad(request.asset, endLoadParam);
                    }
                }
            }
        }

        private IEnumerator CreateAssetBundle_RelationFile(string originName, string destName, Request request, EndLoadBundle endLoad, System.Object ud)
        {
            request.raf = new RelationAssetFile(destName);
            request.raf.Load();

            while (!request.raf.Over())
            {
                yield return null;
            }

            if (request.raf.GetMainAsset() == null)
            {
                if (Config.Debug_Log())
                    Debugger.LogError("Failed to load asset:%s, asset is null", originName);
            }

            request.referenceNum++;
            ProcessRequest(request, (System.Object)endLoad, ud);
        }

        private void CreateAssetBundle(string originName, string destName, string path, Example.VersionFile.Type fileType, Request request, EndLoadBundle endLoad, System.Object ud, bool assetBundle)//, bool cb_whatever)
        {
            if (fileType == Example.VersionFile.Type.DEFAULT)
                CoroutineHelper.CreateCoroutineHelper(CreateAssetBundle_Offset(originName, path, 0, request, endLoad, ud, assetBundle));
            else if (fileType == Example.VersionFile.Type.COMBINE_FILE)
            {
                CombineFile cf = CombineFileManager.GetInstance().GetCombineFile(destName);
                int offset;
                int size;
                bool encrypt;
                cf.GetFileDetail(originName, out offset, out size, out encrypt);

                if (encrypt)
                {
                    Debugger.LogError("asset bundle resource in combine file should not encrypt");
                    return;
                }

                CoroutineHelper.CreateCoroutineHelper(CreateAssetBundle_Offset(originName, path, (ulong)offset, request, endLoad, ud, assetBundle));
            }
        }

        private IEnumerator CreateAssetBundle_Offset(string name, string path, ulong offset, Request request, EndLoadBundle endLoad, System.Object ud, bool assetBundle)//, bool cb_whatever)
        {
            if (Config.Detail_Debug_Log())
                Debug.LogWarning("begin create asset ---------->" + name);

            AssetBundleCreateRequest result = AssetBundle.LoadFromFileAsync(path, 0, offset);

            yield return result;

            if (Config.Detail_Debug_Log())
                Debug.LogWarning("end create asset ----------->" + name);

            if (!result.isDone || result.assetBundle == null)
            {
                if (Config.Debug_Log())
                    Debugger.LogError("Failed to create asset bundle: " + path + "^" + ResUpdate.ConvertPathToName(path));
                cache.Remove(name);
                ProcessRequest(request, (System.Object)endLoad, ud);
                yield break;
            }
            if (!assetBundle)
            {
                request.asset = result.assetBundle.mainAsset;
                if (Config.Detail_Debug_Log())
                {
                    if(result.assetBundle.mainAsset != null)
                        Debug.LogWarning("create asset bundle type->" + result.assetBundle.mainAsset);
                }
            }
            else
            {
                request.asset = result.assetBundle;
            }

            if (request.asset == null)
            {
                UnityEngine.Object[] arr = result.assetBundle.LoadAllAssets();
                if (arr != null && arr.Length > 0)
                {
                    request.asset = arr[0];
                    if (arr.Length > 1)
                    {
                        for (int i = 1, count = arr.Length; i < count; i++)
                        {
                            Resources.UnloadAsset(arr[i]);
                        }
                    }
                }
            }

            if (request.asset == null)
            {
                if (Config.Debug_Log())
                    Debugger.LogError("Failed to load asset:%s, asset is null", ResUpdate.ConvertPathToName(path));
            }
            
            request.referenceNum++;
            ProcessRequest(request, (System.Object)endLoad, ud);

            if (!assetBundle)
            {
                CoroutineHelper.CreateCoroutineHelper(AsynUnloadAssetBundle(result.assetBundle));
            }
        }

        private IEnumerator AsynUnloadAssetBundle(AssetBundle ab)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (ab != null)
                ab.Unload(false);
        }

        #region unload
        private void _UnloadParticleSystem(ParticleSystem ps)
        {
            if (ps == null)
                return;
            if (ps.GetComponent<Renderer>())
            {
                Renderer r = ps.GetComponent<Renderer>();
                if (r.sharedMaterial)
                {
                    if (r.sharedMaterial.mainTexture)
                        Resources.UnloadAsset(r.sharedMaterial.mainTexture);
                    UnityEngine.Object.Destroy(r.sharedMaterial);
                }
            }
        }

        private void _UnloadRenderer(Renderer renderer)
        {
            if (renderer)
            {
                if (renderer.sharedMaterial)
                {
                    if (renderer.sharedMaterial.mainTexture)
                        Resources.UnloadAsset(renderer.sharedMaterial.mainTexture);
                    UnityEngine.Object.Destroy(renderer.sharedMaterial);
                }

                if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;
                    Resources.UnloadAsset(smr.sharedMesh);
                }
            }
        }

        internal void UnloadAsset(UnityEngine.Object asset)
        {
            _UnloadAsset(asset);
        }

        private void _UnloadAsset(UnityEngine.Object asset)
        {
            try
            {
                if (asset == null)
                    return;
                if (!asset)
                    return;
                if (asset is AssetBundle)
                    (asset as AssetBundle).Unload(true);
                else
                {

                    if (asset is GameObject)
                    {
                        GameObject go = asset as GameObject;

                        try
                        {
                            if (go.GetComponent<Renderer>())
                            {

                                _UnloadRenderer(go.GetComponent<Renderer>());
                            }
                            else
                            {
                                UIAtlas atlas = go.GetComponent<UIAtlas>();
                                if (atlas != null)
                                {
                                    if (atlas.spriteMaterial)
                                    {
                                        if (atlas.spriteMaterial.mainTexture)
                                            Resources.UnloadAsset(atlas.spriteMaterial.mainTexture);
                                        Resources.UnloadAsset(atlas.spriteMaterial);
                                    }
                                }
                                else
                                {
                                    ParticleSystem ps = go.GetComponent<ParticleSystem>();
                                    if (ps != null)
                                    {
                                        _UnloadParticleSystem(ps);
                                    }
                                    else
                                    {
                                        ps = go.GetComponentInChildren<ParticleSystem>();
                                        if (ps != null)
                                            _UnloadParticleSystem(ps);
                                        else
                                        {
                                            Renderer r = go.GetComponentInChildren<Renderer>();
                                            if (r)
                                                _UnloadRenderer(r);
                                            else
                                                Debugger.LogWarning("can not unload asset");
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                        }

                        UnityEngine.Object.DestroyImmediate(go, true);
                    }
                    else if (asset is Texture2D)
                    {
                        Resources.UnloadAsset(asset);
                    }
                    else if (asset is Texture)
                    {
                        Resources.UnloadAsset(asset);
                    }
                    else if (asset is AudioClip)
                    {
                        Resources.UnloadAsset(asset);
                    }
                    else if (asset is ScriptableObject)
                    {
                    }
                    else if (asset is Material)
                    {
                        UnityEngine.Object.DestroyImmediate(asset, true);
                        Resources.UnloadAsset(asset);
                    }
                    else
                    {
                        Resources.UnloadAsset(asset);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debugger.LogError("unload asset error->" + e.ToString());
            }
        }

        internal bool RemoveCache(string name, bool immediately = false, bool force = false)
        {
            return _RemoveCache(name, immediately, force);
        }

        private void UnloadRequest(string name, Request request)
        {
            if (request.raf != null)
            {
                request.raf.UnLoad();
                request.raf = null;
            }
            else
            {
                _UnloadAsset(request.asset);
                request.asset = null;
            }
            cache.Remove(name);
        }

        private bool _RemoveCache(string name, bool immediately = false, bool force = false)
        {
            if (name == null || name == "")
                return false;
            if (Config.Detail_Debug_Log())
                Debug.LogWarning("begin remove cache->" + name + "^" + immediately + "^" + force);
            Request request;
            if (cache.TryGetValue(name, out request))
            {
                try
                {
                    if (!force)
                    {
                        request.referenceNum--;

                        if (Config.Detail_Debug_Log())
                            Debug.LogWarning("remove cache reference num->" + request.referenceNum);
                        if (request.referenceNum <= 0)
                        {
                            if (!immediately)
                            {
                                Removing rm = new Removing();
                                rm.request = request;
                                rm.name = name;
                                needUnLoad.Add(rm);
                            }
                            else
                            {
                                UnloadRequest(name, request);
                            }
                        }
                    }
                    else
                    {
                        if (!immediately && (request.asset == null && request.raf == null))
                            return false; 
                        UnloadRequest(name, request);
                    }
                }
                catch (System.Exception e)
                {
                    //#if UNITY_EDITOR
                    Debugger.LogError("remove load cache failed->" + e.ToString());
                    //#endif
                }
                return true;
            }
            else
                return false;
        }
        #endregion

        internal UnityEngine.Object Search(string name)
        {
            return _Search(name);
        }

        private UnityEngine.Object _Search(string name)
        {
            Request request;
            if (cache.TryGetValue(name, out request))
                return request.asset;
            return null;
        }

        public void Update()
        {
            if (needUnLoad.Count > 0)
            {
                Removing rm;
                for (int i = 0; i < needUnLoad.Count; i++)
                {
                    rm = needUnLoad[i];
                    if (rm.request.referenceNum <= 0)
                    {
                        UnloadRequest(rm.name, rm.request);
                    }
                }

                needUnLoad.Clear();
            }
        }
    }
}
