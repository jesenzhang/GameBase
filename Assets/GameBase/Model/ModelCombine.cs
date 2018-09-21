using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Model
{
    class ModelCombine
    {
        struct LoadInfo
        {
            public string name;
            public short index;
            public bool autoTemp;
        }

        private GameObject combinedRoot = null;

        private bool show = false;
        private int layer = 0;

        private int av = -1;
        private int bv = -1;

        private CharacterAsset[] assetArr = null;
        private bool[] replace = null;

        private bool packTex = false;
        private bool dirty = false;

        private Transform body;

        public delegate void EndProcessModel(Transform trans, object param);
        private EndProcessModel endProcessModel;
        private object endProcessModelParam = null;

        private static int assetPartCount = 0;



        public ModelCombine()
        {
            if (assetPartCount > 0)
            {
                replace = new bool[assetPartCount];
                assetArr = new CharacterAsset[assetPartCount];
            }
        }

        internal void SetCombinedRoot(GameObject go)
        {
            combinedRoot = go;
        }

        internal static void SetAssetPartCount(int count)
        {
            assetPartCount = count;
        }

        internal void SetEndProcessModelCall(EndProcessModel process, object param)
        {
            endProcessModel = process;
            endProcessModelParam = param;
        }

        private bool CheckReplace()
        {
            for (int i = 0, count = replace.Length; i < count; i++)
            {
                if (replace[i])
                    return false;
            }

            return true;
        }

        private void Wear_Add(int pos, string model)
        {
            replace[pos] = false;

			if (!string.IsNullOrEmpty(model))
            {
                bool doo = false;
                int index = CharacterAsset.TryNameToID(model);
                if (index >= 0)
                {
                    CharacterAsset ca = assetArr[pos];
                    if (ca != null)
                    {
                        if (index != ca.id)
                            doo = true;
                    }
                    else
                        doo = true;
                }
                else
                    doo = true;

                if (doo)
                    replace[pos] = true;
            }
        }

        private void CombineModel(bool autoTemp)
        {
            List<CharacterAsset> caList = new List<CharacterAsset>();
            CharacterAsset ca = null;
            for (int i = 0; i < assetPartCount; i++)
            {
                ca = assetArr[i];
                if (ca != null)
                {
                    caList.Add(ca);
                }
            }

            GameObject baseObj = combinedRoot;
            if (baseObj == null)
                return;

            xCombine.Combine(caList, baseObj, EndProcess, true, autoTemp, true, packTex);
        }

        private void ClearModel(GameObject obj, int av, int bv, bool pack)
        {
            bool isdofirst = false;
            bool doo = xCombine.RemoveCombined(obj, av, bv, ref isdofirst, pack);
            SkinnedMeshRenderer s = obj.GetComponent<SkinnedMeshRenderer>();
            if (doo)
            {
                if (pack && s != null)
                {
                    if (s.material != null && s.material.mainTexture != null)
                        Object.Destroy(s.material.mainTexture);
                    Object.Destroy(s.sharedMesh);
                }

                Object.Destroy(obj);
            }

            if (!isdofirst)
            {
                if (pack && s != null)
                {
                    Object.Destroy(s.material);
                    Object.Destroy(s.sharedMaterial);
                }
            }
        }

        private void ErgodicTransform(Transform parent, int layer)
        {
            if (parent == null)
                return;
            if (parent.childCount > 0)
            {
                foreach (Transform t in parent)
                {
                    t.gameObject.layer = layer;
                    ErgodicTransform(t, layer);
                }
            }
        }

        private void EndProcess(GameObject obj, int av, int bv, System.Object endParam)
        {
            if (obj == null)
            {
                Debug.LogError("end process model is null");
                return;
            }

            if (endParam == null)
            {
                ClearModel(obj, av, bv, packTex);
                return;
            }

            if (dirty)
            {
                ClearModel(obj, av, bv, packTex);
                return;
            }

            if (body != null)
                ClearModel(body.gameObject, av, bv, packTex);

            if (body && body.gameObject.activeSelf)
                Object.Destroy(body.gameObject);

            body = obj.transform;
            this.av = av;
            this.bv = bv;

            ErgodicTransform(body, layer);


            if (endProcessModel != null)
                endProcessModel(body, endProcessModelParam);
        }

        private void OnCCACallback(CharacterAsset ca, System.Object param)
        {
            if (dirty)
            {
                if (endProcessModel != null)
                    endProcessModel(null, endProcessModelParam);
                return;
            }

            if (ca == null || param == null)
                return;

            LoadInfo loadInfo = (LoadInfo)param;
            if (loadInfo.index < 0 || loadInfo.index >= assetPartCount)
            {
                Debug.LogError("model load index is invalid->" + loadInfo.index);
                return;
            }

            if (replace[loadInfo.index])
            {
                replace[loadInfo.index] = false;
                assetArr[loadInfo.index] = ca;
            }
            else
            {
                Debug.LogError("load index is not in replace->" + loadInfo.index);
                return;
            }

            if (CheckReplace())
            {
                CombineModel(loadInfo.autoTemp);
            }
        }

        private void Wear_Load(int pos, string model, bool autoTemp)
        {
			if (string.IsNullOrEmpty(model))
                return;

            if (!replace[pos])
                return;

            CharacterAsset.CreateCharacterAsset(model, OnCCACallback, new LoadInfo() { index = (short)pos, name = model, autoTemp = autoTemp }, packTex);
        }

        public void Wear(string[] models, bool autoTemp)
        {
            if (models == null)
                return;

            int count = models.Length;
            if (count == 0)
                return;

            string model = null;
            for (int i = 0; i < count; i++)
            {
                model = models[i];
				if (string.IsNullOrEmpty(model))
                    continue;

                Wear_Add(i, model);
            }

            bool doo1 = CheckReplace();
            if (doo1)
            {
                bool doo = false;
                if (body == null)
                {
                    CombineModel(autoTemp);
                    doo = true;
                }

                if (!doo)
                {
                    if (endProcessModel != null)
                        endProcessModel(body, endProcessModelParam);
                    return;
                }
                else
                    return;
            }

            for (int i = 0; i < count; i++)
                Wear_Load(i, models[i], autoTemp);
        }

        private void ClearAssets()
        {
            for (int i = 0; i < assetPartCount; i++)
            {
                assetArr[i] = null;
                replace[i] = false;
            }
        }

        internal void OnDestroy()
        {
        }
    }
}
