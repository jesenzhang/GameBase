
//#define NEW_COMBINE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameBase
{
    public static partial class xCombine
    {
        private class TempLoad_NPT
        {
            public List<CombineInstance> combineInstances;
            public List<Transform> bones;
            public List<Mesh> meshList;
            public List<Material> materials;
            public EndCombine endCombine;
            public System.Object endParam;
            public GameObject root;
            public bool light;
            public int plus;
            public int sub;
        }

        private static void ProcessCombine(TempLoad_NPT tl)
        {
            lock (lockCombine)
            {
                try
                {
                    SkinnedMeshRenderer r = tl.root.GetComponent<SkinnedMeshRenderer>();
                    r.sharedMesh = new Mesh();
                    r.sharedMesh.CombineMeshes(tl.combineInstances.ToArray(), false, false);
                    r.bones = tl.bones.ToArray();
                    r.materials = tl.materials.ToArray();

                    CombineObj co = new CombineObj();
                    co.obj = tl.root;
                    co.combine = true;
                    if (!combinedDic2.ContainsKey(tl.plus))
                    {
                        Dictionary<int, List<CombineObj>> dic = new Dictionary<int, List<CombineObj>>();
                        List<CombineObj> list = new List<CombineObj>();

                        CombineObj rootObj = new CombineObj();
                        rootObj.combine = true;
                        rootObj.obj = (GameObject)Object.Instantiate(co.obj);
                        rootObj.obj.SetActive(false);

                        list.Add(rootObj);

                        co.combine = false;
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
                            CombineObj rootObj = new CombineObj();
                            rootObj.combine = true;
                            rootObj.obj = (GameObject)Object.Instantiate(co.obj);
                            rootObj.obj.SetActive(false);

                            list.Add(rootObj);

                            co.combine = false;
                            list.Add(co);
                            dic.Add(tl.sub, list);
                        }
                    }

                    //add first, then avoid to del
                    if (tl.endCombine != null)
                        tl.endCombine(tl.root, tl.plus, tl.sub, tl.endParam);
                }
                catch (System.Exception e)
                {
                    if (tl.endCombine != null)
                        tl.endCombine(null, tl.plus, tl.sub, tl.endParam);

                    Debug.LogError("process combine error->" + e.ToString());
                }
            }
        }

        private static void _Combine_NPT(CombineInfo combineInfo)
        {
            List<Mesh> meshList = new List<Mesh>();
            try
            {
                CharacterAsset item = null;
                int i, j, k, count, count1, count2;
                List<CombineInstance> combineInstances = new List<CombineInstance>();
                List<Transform> bones = new List<Transform>();
                List<Material> materials = new List<Material>();
                Transform[] transforms = combineInfo.root.GetComponentsInChildren<Transform>();

                SkinnedMeshRenderer smr = null;
                CombineInstance ci;
                string[] strs = null;
                string str = null;
                Transform transform;
                count2 = transforms.Length;
                count = combineInfo.items.Count;
                for (i = 0; i < count; i++)
                {
                    item = combineInfo.items[i];
                    smr = item.GetSkinnedMeshRenderer();
                    if (smr == null)
                        return;

                    materials.AddRange(smr.materials);

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

                TempLoad_NPT tl = new TempLoad_NPT();
                tl.combineInstances = combineInstances;
                tl.bones = bones;
                tl.meshList = meshList;
                tl.materials = materials;
                tl.endCombine = combineInfo.endCombine;
                tl.endParam = combineInfo.endParam;
                tl.root = combineInfo.root;
                tl.plus = combineInfo.plus;
                tl.sub = combineInfo.sub;
                tl.light = combineInfo.light;

                ProcessCombine(tl);
            }
            catch (System.Exception e)
            {
                if (combineInfo != null && combineInfo.endCombine != null)
                    combineInfo.endCombine(null, -1, -1, combineInfo.endParam);
                Debug.LogError("combine error->" + e.ToString() + "\r\n" + e.StackTrace);
            }
        }
    }
}