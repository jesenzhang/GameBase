
//#define NEW_COMBINE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameBase
{
    public static partial class xCombine
    {
        private class TexInfo
        {
            public string name;
            public Texture2D tex;
            public short w;
            public short h;
        }

        private class TempLoad
        {
            public TexInfo[] texInfo;
            public List<CombineInstance> combineInstances;
            public List<Transform> bones;
            public List<Mesh> meshList;
            public EndCombine endCombine;
            public System.Object endParam;
            public GameObject root;
            public short index;
            public bool over;
            public bool autoTemp;
            public bool light;
            public int plus;
            public int sub;
            public string[] texName;
        }

        private static object lockCombine = new object();
        private static object lockClone = new object();

        public delegate void EndCombine(GameObject obj, int a, int b, System.Object endParam);

        private static void OnLoad(UnityEngine.Object asset, System.Object obj)
        {
            TempLoad tl = (TempLoad)obj;
            try
            {
                Texture2D tex = (Texture2D)asset;

                TexInfo texInfo = new TexInfo();
                texInfo.tex = tex;
                texInfo.w = (short)tex.width;
                texInfo.h = (short)tex.height;
                tl.texInfo[tl.index] = texInfo;

                bool over = true;
                int i, count;
                TexInfo ti;
                for (i = 0, count = tl.texInfo.Length; i < count; i++)
                {
                    ti = tl.texInfo[i];
                    if (ti == null)
                    {
                        over = false;
                        break;
                    }
                }
                tl.over = over;

                if (over)
                {
                    if (tl.autoTemp)
                        PackCombineTexture(tl);
                    else
                    {
                        Texture2D texture = new Texture2D(1, 1);

                        Rect[] rect = CreateTex(texture, tl.texInfo);

                        ProcessCombine(tl, texture, rect);
                    }
                }
            }
            catch (System.Exception e)
            {
                for (int i = 0; i < tl.meshList.Count; i++)
                    Object.Destroy(tl.meshList[i]);
                if (tl.endCombine != null)
                    tl.endCombine(null, tl.plus, tl.sub, tl.endParam);

                Debug.LogError("combine load e->" + asset.name + "^" + e.ToString());
            }
        }

        private static void ProcessCombine(TempLoad tl, Texture2D texture, Rect[] rect)
        {
            lock (lockCombine)
            {
                int i, count, j = 0, k = 0, count1;
                Rect rt;
                TexInfo ti;
                Mesh mesh;
                Vector2[] uvs;

                for (i = 0, count = tl.meshList.Count; i < count; i++)
                {
                    rt = rect[i];
                    ti = tl.texInfo[i];
                    mesh = tl.meshList[i];

                    uvs = mesh.uv;
                    count1 = uvs.Length;
                }

                try
                {
                    texture.Compress(false);
                    if (rect == null)
                    {
                        Debug.LogError("rect is null");
                        return;
                    }

                    Vector2 v2;
                    Vector2 nuv;

                    for (i = 0, count = tl.meshList.Count; i < count; i++)
                    {
                        rt = rect[i];
                        ti = tl.texInfo[i];
                        mesh = tl.meshList[i];

                        uvs = mesh.uv;

                        count1 = uvs.Length;
                        for (j = 0; j < count1; j++)
                        {
                            v2 = uvs[j];
                            nuv = new Vector2(rt.xMin + v2.x * ti.w / texture.width, rt.yMin + v2.y * ti.h / texture.height);
                            uvs[j] = nuv;
                        }
                        mesh.uv = uvs;
                    }

                    SkinnedMeshRenderer r = tl.root.GetComponent<SkinnedMeshRenderer>();
                    r.sharedMesh = new Mesh();
                    r.sharedMesh.CombineMeshes(tl.combineInstances.ToArray(), true, false);
                    r.bones = tl.bones.ToArray();

                    Material material = new Material("xx");
                    material.mainTexture = texture;

                    r.materials = new Material[] { material };

                    CombineObj co = new CombineObj();
                    co.obj = tl.root;
                    co.combine = true;
                    if (!combinedDic2.ContainsKey(tl.plus))
                    {
                        Dictionary<int, List<CombineObj>> dic = new Dictionary<int, List<CombineObj>>();
                        List<CombineObj> list = new List<CombineObj>();
                        list.Add(co);
                        dic.Add(tl.sub, list);
                        combinedDic2.Add(tl.plus, dic);
                    }
                    else
                    {
                        Dictionary<int, List<CombineObj>> dic = combinedDic2[tl.plus];
                        if (dic.ContainsKey(tl.sub))
                            dic[tl.sub].Add(co);
                        else
                        {
                            List<CombineObj> list = new List<CombineObj>();
                            list.Add(co);
                            dic.Add(tl.sub, list);
                        }
                    }

                    //add first, then avoid to del
                    if (tl.endCombine != null)
                        tl.endCombine(tl.root, tl.plus, tl.sub, tl.endParam);

                    //Mesh mesh;
                    for (i = 0, count = tl.meshList.Count; i < count; i++)
                    {
                        Object.Destroy(tl.meshList[i]);
                    }

                    for (i = 0, count = tl.texName.Length; i < count; i++)
                        ResLoader.RemoveAssetCacheByName(tl.texName[i]);
                }
                catch (System.Exception e)
                {
                    for (i = 0, count = tl.meshList.Count; i < count; i++)
                    {
                        Object.Destroy(tl.meshList[i]);
                    }

                    for (i = 0, count = tl.texName.Length; i < count; i++)
                        ResLoader.RemoveAssetCacheByName(tl.texName[i]);

                    if (tl.endCombine != null)
                        tl.endCombine(null, tl.plus, tl.sub, tl.endParam);

                    Debug.LogError("process combine error->" + e.ToString());
                }
            }
        }

        private static Rect[] CreateTex(Texture2D tex, TexInfo[] texInfo)
        {
            Texture2D[] texArr = new Texture2D[texInfo.Length];
            int i = 0, count;
            TexInfo ti;
            for (i = 0, count = texInfo.Length; i < count; i++)
            {
                ti = texInfo[i];
                if (ti == null || ti.tex == null)
                {
                    Debug.LogError("tex is null");
                    return null;
                }

                texArr[i] = ti.tex;
            }

            Rect[] rect = tex.PackTextures(texArr, 0);

            return rect;
        }

        class UseInfo
        {
            public GameObject obj;
            public short num;
            public bool isUsing;
        }

        class CombineObj
        {
            public GameObject obj;
            public bool combine;
        }
        private static Dictionary<int, Dictionary<int, List<CombineObj>>> combinedDic2 = new Dictionary<int, Dictionary<int, List<CombineObj>>>();

        private static void RemoveSMR(SkinnedMeshRenderer smr)
        {
            if (smr == null)
                return;
            ResLoader.Unload(smr.material.mainTexture);
            ResLoader.Unload(smr.material);
            if (smr.sharedMaterial != null)
            {
                ResLoader.Unload(smr.sharedMaterial.mainTexture);
                ResLoader.Unload(smr.sharedMaterial);
            }
            ResLoader.Unload(smr.sharedMesh);
        }

        public static bool RemoveCombined(GameObject obj, int a, int b, ref bool isdofirst, bool packTex)
        {
            lock (lockClone)
            {
                if (combinedDic2.ContainsKey(a))
                {
                    Dictionary<int, List<CombineObj>> dic = combinedDic2[a];
                    if (dic.ContainsKey(b))
                    {
                        List<CombineObj> list = dic[b];
                        CombineObj co, coo;
                        for (int i = 0; i < list.Count; i++)
                        {
                            co = list[i];
                            if (!co.obj || co.obj == null)
                            {
                                list.RemoveAt(i);
                                i--;
                                continue;
                            }

                            if (co.obj == obj)
                            {
                                if (co.combine)
                                {
                                    bool doo = true;
                                    if (i == 0)
                                    {
                                        for (int j = 0; j < list.Count; j++)
                                        {
                                            coo = list[j];
                                            if (!coo.combine && coo.obj != null)
                                            {
                                                doo = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (!doo)
                                    {
                                        isdofirst = true;
                                        co.obj.transform.parent = null;
                                        co.obj.SetActive(false);
                                        continue;
                                    }
                                    SkinnedMeshRenderer smr = obj.GetComponent<SkinnedMeshRenderer>();
                                    if (packTex && smr != null)
                                    {
                                        RemoveSMR(smr);
                                    }
                                    Object.Destroy(obj);
                                    list.RemoveAt(i);
                                    i--;
                                }
                                else
                                {

                                    bool doo = true;
                                    bool doo1 = true;
                                    for (int j = 0; j < list.Count; j++)
                                    {
                                        coo = list[j];
                                        if (coo == co)
                                            continue;
                                        if (!coo.combine)
                                        {
                                            doo = false;
                                            break;
                                        }
                                        if (coo.obj != null && coo.obj.activeSelf)
                                        {
                                            doo1 = false;
                                            break;
                                        }
                                    }

                                    if (doo && doo1)
                                    {
                                        for (int j = 0; j < list.Count; j++)
                                        {
                                            co = list[j];
                                            if (co.obj != null)
                                            {
                                                {
                                                    SkinnedMeshRenderer smr = co.obj.GetComponent<SkinnedMeshRenderer>();
                                                    if (packTex && smr != null)
                                                    {
                                                        RemoveSMR(smr);
                                                    }
                                                }
                                                Object.Destroy(co.obj);
                                            }
                                        }

                                        list.Clear();
                                    }
                                    else
                                    {
                                        Object.Destroy(co.obj);
                                        co.obj = null;

                                        list.RemoveAt(i);
                                        i--;
                                    }
                                }
                            }
                        }
                    }
                    else
                        return true;
                }
                else
                    return true;

                return false;
            }
        }

        public static void RemoveAll()
        {
            combinedDic2.Clear();

            combineQueue.Clear();
            piQueue.Clear();

            curpi = null;
            if (curTex != null)
            {
                ResLoader.Unload(curTex);
                curTex = null;
            }
        }

        private class CombineInfo
        {
            public List<CharacterAsset> items;
            public GameObject root;
            public EndCombine endCombine;
            public System.Object endParam;
            public int plus;
            public int sub;
            public bool autoTemp;
            public bool light;
            public bool packTex;
        }

        private static Queue<CombineInfo> combineQueue = new Queue<CombineInfo>();

        public static void Combine(List<CharacterAsset> items, GameObject root, EndCombine endCombine, System.Object endParam, bool autoTemp, bool light, bool packTex)
        {
            lock (lockClone)
            {
                try
                {
                    int plus = 0;
                    int sub = 0;
                    int i, iid;
                    CharacterAsset item = null;
                    int count = (short)items.Count;
                    for (i = 0; i < count; i++)
                    {
                        item = items[i];
                        iid = item.id;
                        
                        plus += iid;
                        if (sub == 0)
                            sub = iid;
                        else
                            sub -= iid;
                    }

                    if (combinedDic2.ContainsKey(plus))
                    {
                        Dictionary<int, List<CombineObj>> dic = combinedDic2[plus];
                        if (dic.ContainsKey(sub))
                        {
                            List<CombineObj> list = dic[sub];
                            if (list.Count > 0)
                            {
                                try
                                {
                                    if (!list[0].obj)
                                    {
                                        for (int m = 1; m < list.Count; m++)
                                        {
                                            if (list[m] == null || !list[m].obj)
                                            {
                                                list.RemoveAt(m);
                                                m--;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        GameObject go = (GameObject)Object.Instantiate(list[0].obj);
                                        if (!go.activeSelf)
                                            go.SetActive(true);

                                        CombineObj co = new CombineObj();
                                        co.obj = go;
                                        co.combine = false;
                                        list.Add(co);

                                        //add first, then avoid to del in callback
                                        if (endCombine != null)
                                            endCombine(go, plus, sub, endParam);

                                        Object.Destroy(root);
                                        items.Clear();
                                        return;
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError("combine body failed 3->" + e.ToString() + "^" + list.Count);
                                }
                            }
                        }
                    }

                    CombineInfo ci = new CombineInfo();
                    ci.items = items;
                    ci.root = root;
                    ci.endCombine = endCombine;
                    ci.endParam = endParam;
                    ci.plus = plus;
                    ci.sub = sub;
                    ci.autoTemp = autoTemp;
                    ci.light = light;
                    ci.packTex = packTex;
                    combineQueue.Enqueue(ci);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("combine error->" + e.ToString());
                }
            }
        }

        private static void _Combine(CombineInfo combineInfo)
        {
            List<Mesh> meshList = new List<Mesh>();
            try
            {
                CharacterAsset item = null;
                int i, j, k, count, count1, count2;
                List<CombineInstance> combineInstances = new List<CombineInstance>();
                List<Transform> bones = new List<Transform>();
                Transform[] transforms = combineInfo.root.GetComponentsInChildren<Transform>();


                TexInfo[] texInfo = new TexInfo[combineInfo.items.Count];
                SkinnedMeshRenderer smr = null;
                CombineInstance ci;
                string[] strs = null;
                string str = null;
                Transform transform;
                count2 = transforms.Length;
                count = (short)combineInfo.items.Count;
                for (i = 0; i < count; i++)
                {
                    item = combineInfo.items[i];
                    smr = item.GetSkinnedMeshRenderer();
                    if (smr == null)
                        return;
                    Mesh mesh = Mesh.Instantiate(smr.sharedMesh) as Mesh;
                    for (j = 0, count1 = smr.sharedMesh.subMeshCount; j < count1; j++)
                    {
                        ci = new CombineInstance();
                        ci.mesh = mesh;
                        ci.subMeshIndex = j;
                        combineInstances.Add(ci);
                    }

                    strs = item.GetBoneNames();
                    for (j = 0, count1 = strs.Length; j < count1; j++)
                    {
                        str = strs[j];
                        for (k = 0; k < count2; k++)
                        {
                            transform = transforms[k];
                            if (transform.name != str)
                                continue;
                            bones.Add(transform);
                            break;
                        }
                    }

                    meshList.Add(mesh);

                    Object.Destroy(smr.gameObject);
                }

                TempLoad tl = null;
                string[] strArr = new string[count];

                string destName;
                string path;
                int size;
                bool encrypt;
                Example.VersionFile.Type fileType;
                for (i = 0; i < count; i++)
                {
                    item = combineInfo.items[i];
                    strs = item.GetTexNames();
                    tl = new TempLoad();
                    tl.texInfo = texInfo;
                    tl.index = (short)i;
                    tl.combineInstances = combineInstances;
                    tl.bones = bones;
                    tl.meshList = meshList;
                    tl.endCombine = combineInfo.endCombine;
                    tl.endParam = combineInfo.endParam;
                    tl.root = combineInfo.root;
                    tl.over = false;
                    tl.plus = combineInfo.plus;
                    tl.sub = combineInfo.sub;
                    tl.autoTemp = combineInfo.autoTemp;
                    tl.texName = strArr;
                    tl.light = combineInfo.light;

                    GameUtils.stringBuilder.Remove(0, GameUtils.stringBuilder.Length);
                    GameUtils.stringBuilder.Append(strs[0]);
                    
                    ResUpdate.GetLoadDetails(GameUtils.stringBuilder.ToString(), out destName, out path, out size, out encrypt, out fileType);
                    strArr[i] = GameUtils.stringBuilder.ToString();
                    ResLoader.LoadByPath(strArr[i], destName, path, fileType, size, OnLoad, tl, combineInfo.autoTemp);
                }
            }
            catch (System.Exception e)
            {
                for (int i = 0; i < meshList.Count; i++)
                    Object.Destroy(meshList[i]);
                if (combineInfo != null && combineInfo.endCombine != null)
                    combineInfo.endCombine(null, -1, -1, combineInfo.endParam);
                Debug.LogError("combine error->" + e.ToString());
            }
        }


        class PackInfo
        {
            public TempLoad tl;
            public Rect[] rects;
            public List<byte> texil;
        }
        private static Queue<PackInfo> piQueue = new Queue<PackInfo>();
        private static PackInfo curpi = null;
        private static Texture2D curTex = null;
        private const short TEX_W = 1024;
        private const short TEX_H = 512;

        private static bool inited = false;

        public static void Init()
        {
            if (inited)
                return;

            inited = true;

            Timer.CreateTimer(0.2f, -1, OnUpdate, null);
        }

        private static void CreateTexture2D()
        {
            curTex = new Texture2D(TEX_W, TEX_H, TextureFormat.RGB24, false);
        }

        private static void PackCombineTexture(TempLoad tl)
        {
        }

        private static float prevSetPixelTime = 0f;

        private static void OnUpdate(System.Object obj)
        {
            if (combineQueue.Count > 0)
            {
                CombineInfo ci = combineQueue.Peek();
                bool doo = true;
                CharacterAsset ca;
                for (int i = 0, count = ci.items.Count; i < count; i++)
                {
                    ca = ci.items[i];
                    if (!ca.Check())
                        doo = false;
                }

                if (doo)
                {
                    combineQueue.Dequeue();
                    if (ci.packTex)
                        _Combine(ci);
                    else
                        _Combine_NPT(ci);
                }
            }

            if (curpi != null)
            {
                try
                {
                    if (curpi.texil.Count > 0)
                    {
                        if (Time.realtimeSinceStartup - prevSetPixelTime > 1f)
                        {
                            byte index = curpi.texil[0];
                            Texture2D tex = curpi.tl.texInfo[index].tex;
                            Rect rect = curpi.rects[index];
                            Color[] colors = tex.GetPixels(0, 0, (int)rect.width, (int)rect.height);
                            curTex.SetPixels((int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height, colors);
                            curpi.texil.RemoveAt(0);
                            prevSetPixelTime = Time.realtimeSinceStartup;
                        }
                    }
                    else
                    {
                        //do
                        if (curTex == null)
                        {
                            return;
                        }

                        Rect rect;
                        for (short i = 0, count = (short)curpi.tl.texInfo.Length; i < count; i++)
                        {
                            rect = curpi.rects[i];
                            rect.xMin = rect.xMin / TEX_W;
                            rect.yMin = rect.yMin / TEX_H;
                            curpi.rects[i] = rect;
                        }
                        curTex.Apply();
                        ProcessCombine(curpi.tl, curTex, curpi.rects);

                        if (piQueue.Count > 0)
                        {
                            curpi = piQueue.Dequeue();
                            CreateTexture2D();
                        }
                        else
                        {
                            curpi = null;
                            curTex = null;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    if (curpi != null)
                    {
                        if (curpi.tl != null && curpi.tl.endCombine != null)
                            curpi.tl.endCombine(null, -1, -1, curpi.tl.endParam);
                    }
                    if (piQueue.Count > 0)
                        curpi = piQueue.Dequeue();
                    else
                        curpi = null;
                    Debug.LogError("create texture error->" + e.ToString());
                }
            }
        }
    }
}