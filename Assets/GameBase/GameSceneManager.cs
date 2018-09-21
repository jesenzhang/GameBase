
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.SceneManagement;

namespace GameBase
{
    public class GameSceneManager : MonoBehaviour
    {
        public enum LoadSceneStatus
        {
            NONE,
            BEGIN,
            END,
        }

#if LUASCRIPT
        LuaInterface.LuaFunction luaLoadOverFunc = null;
#endif

        private static bool inited = false;

        private static List<AssetBundle> sceneAsset = new List<AssetBundle>();
        private static List<string> c_scene = new List<string>();

        private static LoadSceneStatus loadStatus = LoadSceneStatus.NONE;

        private static GameSceneManager sceneManager = null;
        private static bool sceneLoading = false;

        private static string _next = "0";
        private static string next
        {
            get { return _next; }
            set { _next = value; }
        }

        private static int curNum = 0;

        private static bool first = true;
        private bool alive = false;


        void Awake()
        {
            if (inited)
                return;
            sceneManager = this;
            inited = true;
            alive = true;

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                OnSceneWasLoaded();
            else
                OnAdditiveSceneLoaded(scene.name);
        }

        private void OnSceneUnloaded(Scene scene)
        {
        }

        public static void LoadScene(string scene)
        {
            if (scene == null || scene == "")
                return;

            //prev clean
            if (loadStatus != LoadSceneStatus.NONE)
                return;

            next = scene;

            loadStatus = LoadSceneStatus.BEGIN;

            System.GC.Collect();

            if (!first)
            {
                UIFrame.SceneLoadProcessAtlases();
                sceneManager.xLoadScene("1");
            }
            else
            {
                first = false;
                sceneManager.NextLoadingScene();
            }
        }

        private static void DirectlyLoadScene()
        {
            if (next == null || next == "")
                return;

            if (loadStatus != LoadSceneStatus.END)
                return;

            if (sceneManager == null)
                return;

            sceneManager.xLoadScene(next);
        }

        private void NextLoadingScene()
        {
            if (!alive)
                return;
            OnSceneWasLoaded();
        }

        private void xLoadScene(string scene)
        {
            if (!alive)
                return;
            StartCoroutine(DoLoadScene(scene));
        }

        private IEnumerator DoLoadScene(string scene)
        {
            if (!alive)
                yield break;
            yield return new WaitForEndOfFrame();

            //begin load scene clear
            GPUBillboardBuffer_S.Instance().OnLeaveStage();

            AsyncOperation asy = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
            yield return asy;

            Time.timeScale = 1;
        }

        private static void ClearSceneAsset()
        {
            for (int i = 0, count = sceneAsset.Count; i < count; i++)
            {
                if (sceneAsset[i])
                    sceneAsset[i].Unload(false);

                bool re = ResLoader.RemoveAssetCacheByName(c_scene[i], true, true);
            }

            sceneAsset.Clear();
            c_scene.Clear();
        }

        private void EndLoad(Object ab, object obj)
        {
            if (!alive)
                return;
            if (!(ab is AssetBundle))
                return;

            AssetBundle asset = ab as AssetBundle;
            sceneAsset.Add(asset);
            c_scene.Add(next);

            loadStatus = LoadSceneStatus.END;
            DirectlyLoadScene();
        }

        private void Load()
        {
            if (!alive)
                return;
            if (next == null || next == "")
                return;

            ResLoader.LoadByName(next, EndLoad, next, true);
        }

        private void OnSceneWasLoaded()
        {
            if (!alive)
                return;
            if (loadStatus == LoadSceneStatus.END)
            {
                {
                    loadStatus = LoadSceneStatus.NONE;
                    sceneLoading = false;

                    LuaContext.RefreshDelegateMap();

#if LUASCRIPT
                    if (luaLoadOverFunc == null)
                    {
                        luaLoadOverFunc = LuaManager.GetFunction("SceneManager.ListenLoadOver");
                    }

                    if (luaLoadOverFunc != null)
                        LuaManager.CallFunc_VX(luaLoadOverFunc, next);
#endif
                }
            }
            else if (loadStatus == LoadSceneStatus.BEGIN)
            {
                if (!sceneLoading)
                {
                    sceneLoading = true;

                    if (next != null)
                        Load();
                }
            }

            ClearSceneAsset();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        #region ADDITIVE
        class AdditiveInfo
        {
            internal int index;
            internal float beginUnloadTime;
        }

        private static List<AssetBundle> additiveSceneAssets = new List<AssetBundle>();
        private static Dictionary<string, AdditiveInfo> additiveSceneName = new Dictionary<string, AdditiveInfo>();

        private static float additiveCacheTime = 5;

#if LUASCRIPT
        LuaInterface.LuaFunction luaAdditiveLoadOverFunc = null;
#endif


        public static void SetAdditiveCacheTime(float time)
        {
            additiveCacheTime = time;
        }

        public static void LoadSceneAdditive(string scene)
        {
            if (scene == null || scene == "")
                return;

            sceneManager.LoadAdditive(scene);
        }

        public static void UnLoadSceneAdditive(string scene)
        {
            if (scene == null || scene == "")
                return;
            AdditiveInfo info;
            if (!additiveSceneName.TryGetValue(scene, out info))
                return;

            if (info == null)
            {
                additiveSceneName.Remove(scene);
                return;
            }

            info.beginUnloadTime = Time.realtimeSinceStartup;
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
        }

        private int FindEmptyAdditiveSceneAssetsListPos()
        {
            for (int i = 0, count = additiveSceneAssets.Count; i < count; i++)
            {
                if (additiveSceneAssets[i] == null)
                    return i;
            }

            return -1;
        }

        private void EndLoadAdditive(Object ab, object obj)
        {
            if (!alive)
                return;
            if (!(ab is AssetBundle))
                return;

            string scene = (string)obj;
            AssetBundle asset = ab as AssetBundle;

            AdditiveInfo info;
            if (!additiveSceneName.TryGetValue(scene, out info)) //change to other main scene, etc
            {
                asset.Unload(true);
                ResLoader.RemoveAssetCacheByName(scene);
                return;
            }

            int index = FindEmptyAdditiveSceneAssetsListPos();
            if (index < 0)
            {
                index = additiveSceneAssets.Count;
                additiveSceneAssets.Add(null);
            }
            info.index = index;
            info.beginUnloadTime = -1;
            additiveSceneAssets[index] = asset;

            AdditiveLoadScene(scene);
        }

        private void LoadAdditive(string scene)
        {
            if (!alive)
                return;
            if (scene == null || scene == "")
                return;

            AdditiveInfo info;
            if (additiveSceneName.TryGetValue(scene, out info))
            {
                if(info.beginUnloadTime < 0)
                    return;
            }

            if (info == null) //no cache
            {
                additiveSceneName.Add(scene, new AdditiveInfo() { index = -1, beginUnloadTime = -1 });
                ResLoader.LoadByName(scene, EndLoadAdditive, scene, true);
            }
            else
            {
                AssetBundle asset = additiveSceneAssets[info.index];
                info.beginUnloadTime = -1;
                AdditiveLoadScene(scene);
            }
        }

        private void OnAdditiveSceneLoaded(string name)
        {
#if LUASCRIPT
            if (luaAdditiveLoadOverFunc == null)
            {
                luaAdditiveLoadOverFunc = LuaManager.GetFunction("SceneManager.ListenAdditiveLoadOver");
            }

            if (luaAdditiveLoadOverFunc != null)
                LuaManager.CallFunc_VX(luaAdditiveLoadOverFunc, name);
#endif
        }

        private void AdditiveLoadScene(string scene)
        {
            if (!alive)
                return;
            StartCoroutine(DoAdditiveLoadScene(scene));
        }

        private IEnumerator DoAdditiveLoadScene(string scene)
        {
            if (!alive)
                yield break;
            yield return new WaitForEndOfFrame();

            AsyncOperation asy = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            yield return asy;
        }

        #endregion

        void Update()
        {
            if (this != sceneManager)
                return;

            if (additiveSceneName.Count > 0)
            {
                Dictionary<string, AdditiveInfo>.Enumerator e = additiveSceneName.GetEnumerator();
                float curTime = Time.realtimeSinceStartup;
                List<string> list = new List<string>(additiveSceneName.Count / 2 + 1);
                while (e.MoveNext())
                {
                    if (e.Current.Value.beginUnloadTime >= 0)
                    {
                        if (curTime - e.Current.Value.beginUnloadTime >= additiveCacheTime) //real unload
                        {
                            AssetBundle ab = additiveSceneAssets[e.Current.Value.index];
                            additiveSceneAssets[e.Current.Value.index] = null;
                            ab.Unload(true);
                            ResLoader.RemoveAssetCacheByName(e.Current.Key);
                            list.Add(e.Current.Key);
                        }
                    }
                }

                for (int i = 0, count = list.Count; i < count; i++)
                {
                    if (list[i] != null)
                        additiveSceneName.Remove(list[i]);
                }
            }
        }
    }
}
