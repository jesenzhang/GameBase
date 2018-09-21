using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace GameBase
{
    public class CharacterAsset
    {
        private string _name = null;
        public string name 
        {
            get { return _name; }
        }

        private int _id = -1;
        public int id
        {
            get { return _id; }
        }

        struct LoadInfo
        {
            internal string name;
            internal CCACallback callback;
            internal System.Object param;
            internal bool pack;
        }

        private bool pack = true;
        private GameObject gameObject;
        private StringContentHolder boneName;
        private StringContentHolder textures;

        private UnityEngine.Object goAsset;

        private AssetBundle assetBundle;

        private static Dictionary<int, CharacterAsset> assetArr = new Dictionary<int, CharacterAsset>();
        private static Dictionary<string, int> strToID = new Dictionary<string, int>();


        internal static int TryNameToID(string name)
        {
            int index = -1;
            if (strToID.TryGetValue(name, out index))
                return index;
            else
                return -1;
        }

        private static int NameToID(string name)
        {
            int index = -1;
            if (!strToID.TryGetValue(name, out index))
            {
                index = strToID.Count;
                strToID.Add(name, index);
                return index;
            }
            else
                return -1;
        }

        public static void Clear()
        {
            Dictionary<int, CharacterAsset>.Enumerator e = assetArr.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.OnDestroy();
            }

            strToID.Clear();
            assetArr.Clear();
        }

        public delegate void CCACallback(CharacterAsset ca, System.Object param);

        private static void OnLoad(UnityEngine.Object asset, System.Object param)
        {
            if (param == null)
                return;
            LoadInfo li = (LoadInfo)param;
            if (asset == null)
            {
                Debug.LogError("load character failed->" + li.name);
                return;
            }

            CharacterAsset ca = null;
            int index = -1;
            if (strToID.TryGetValue(li.name, out index))
            {
                if (index >= 0)
                {
                    assetArr.TryGetValue(index, out ca);
                    if (ca != null)
                    {
                        if (li.callback != null)
                            li.callback(ca, li.param);
                        return;
                    }
                }
            }

            ca = new CharacterAsset(asset, li.name, li.pack);
            assetArr.Add(ca.id, ca);

            if (li.callback != null)
                li.callback(ca, li.param);
        }

        public static void CreateCharacterAsset(string name, CCACallback callback, System.Object param, bool pack = true)
        {
            if (callback == null)
                return;
			if (string.IsNullOrEmpty(name))
                return;

            CharacterAsset ca = null;
            int index = -1;
            if (strToID.TryGetValue(name, out index))
            {
                if (index >= 0)
                {
                    assetArr.TryGetValue(index, out ca);
                    if (ca != null)
                    {
                        if (callback != null)
                            callback(ca, param);
                        return;
                    }
                }
            }

            ResLoader.LoadByName(name, OnLoad, new LoadInfo() { name = name, callback = callback, param = param, pack = pack }, true);
        }

        private CharacterAsset(UnityEngine.Object asset, string name, bool pack)
        {
            if (asset is AssetBundle)
            {
                AssetBundle ab = asset as AssetBundle;
                _name = name;
                _id = NameToID(name);
                assetBundle = ab;
                this.pack = pack;

                ResLoader.HelpLoadAsset(ab, "bonenames", LoadBone, null, typeof(StringContentHolder));
            }
        }

        private void LoadBone(AssetBundleRequest abr, System.Object param)
        {
            if (abr == null)
                return;
            boneName = abr.asset as StringContentHolder;

            if (pack)
                ResLoader.HelpLoadAsset(assetBundle, "textures", LoadTex, null, typeof(StringContentHolder));
            else
                ResLoader.HelpLoadAsset(assetBundle, "rendererobject", LoadCallback, null, typeof(GameObject));

        }

        private void LoadTex(AssetBundleRequest abr, System.Object param)
        {
            if (abr == null)
                return;
            textures = abr.asset as StringContentHolder;

            ResLoader.HelpLoadAsset(assetBundle, "rendererobject", LoadCallback, null, typeof(GameObject));
        }

        private void LoadCallback(AssetBundleRequest abr, System.Object param)
        {
            if (abr == null)
                return;
            goAsset = abr.asset;
            gameObject = (GameObject)Object.Instantiate(abr.asset);
            gameObject.SetActive(false);
            if (pack)
            {
                Renderer renderer = gameObject.GetComponent<Renderer>();
                if (renderer != null)
                    Object.Destroy(renderer.material.mainTexture);
            }
            Object.DontDestroyOnLoad(gameObject);
        }

        public bool Check()
        {
            if (pack)
            {
                if (gameObject == null || boneName == null || textures == null)
                    return false;
            }
            else
            {
                if (gameObject == null || boneName == null)
                    return false;
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(false);
                assetBundle = null;
            }
            return true;
        }

        public void OnDestroy()
        {
            try
            {
                if (assetBundle != null)
                {
                    assetBundle.Unload(true);
                    assetBundle = null;
                }

                Object.Destroy(gameObject);
                gameObject = null;

                ResLoader.Unload(goAsset);
                boneName = null;
                textures = null;

                ResLoader.RemoveAssetCacheByName(name, true, true);
            }
            catch (System.Exception e)
            {
                Debug.LogError("unload asset failed->" + e.ToString());
            }
        }

        public SkinnedMeshRenderer GetSkinnedMeshRenderer()
        {
            if (gameObject == null)
                return null;
            GameObject go = (GameObject)Object.Instantiate(gameObject);
            SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
            return smr;
        }

        public string[] GetBoneNames()
        {
            if (boneName == null)
                return null;
            return boneName.content;
        }

        public string[] GetTexNames()
        {
            if (textures == null)
                return null;
            return textures.content;
        }
    }
}