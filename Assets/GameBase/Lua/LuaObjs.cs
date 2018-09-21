using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public static class LuaObjs
    {
        struct GoObj
        {
            internal GameObject go;
            internal Transform trans;
            internal SkinnedMeshRenderer[] smrArr;
            internal MeshRenderer[] rendererArr;
            internal bool temp;
        }

        private const byte ACTIVE = 1;
        private const byte DEACTIVE = 2;

        private static List<GoObj> goList = new List<GoObj>();
        private static List<byte> goActive = new List<byte>();
        private static Queue<int> indexIdle = new Queue<int>();


        public static int RegisterGameObject(GameObject go, bool temp)
        {
            if (go == null)
                return -1;

            int index = -1;
            if (indexIdle.Count > 0)
                index = indexIdle.Dequeue();

            GoObj obj = new GoObj();
            obj.go = go;
            obj.trans = go.transform;
            obj.smrArr = null;
            obj.rendererArr = null;
            obj.temp = temp;

            if (index < 0)
            {
                index = goList.Count;
                goList.Add(obj);
                goActive.Add(ACTIVE);
            }
            else
            {
                goList[index] = obj;
                goActive[index] = ACTIVE;
            }

            return index;
        }

        public static int RegisterTransform(Transform trans, bool temp)
        {
            if (!trans)
                return -100;
            return RegisterGameObject(trans.gameObject, temp);
        }

        public static void UnRegister(int id)
        {
            if (id < 0 || id >= goActive.Count)
                return;

            if (goActive[id] == DEACTIVE)
                return;

            goActive[id] = DEACTIVE;
            indexIdle.Enqueue(id);
        }

        public static void Destroy(int id)
        {
            if (id < 0 || id >= goActive.Count)
                return;

            if (goActive[id] == DEACTIVE)
                return;

            goActive[id] = DEACTIVE;
            indexIdle.Enqueue(id);

            GoObj go = goList[id];
            if (go.go)
            {
                Object.Destroy(go.go);
            }
        }

        public static int InstantiateGameObject(int model, bool temp)
        {
            GameObject go = GetGameObject(model);
            if (!go)
                return -1;

            go = Object.Instantiate(go);
            int index = RegisterGameObject(go, temp);

            return index;
        }

        public static int InstantiateGameObject(GameObject model, bool temp)
        {
            if (!model)
                return -1;

            GameObject go = Object.Instantiate(model);
            int index = RegisterGameObject(go, temp);

            return index;
        }

        public static int NewGameObject(bool temp)
        {
            GameObject go = new GameObject();
            if (!temp)
                Object.DontDestroyOnLoad(go);

            int index = RegisterGameObject(go, temp);

            return index;
        }

        public static int NewGameObject(bool temp, Vector3 pos, Quaternion rotation)
        {
            GameObject go = new GameObject();
            Transform trans = go.transform;
            trans.position = pos;
            trans.rotation = rotation;
            if (!temp)
                Object.DontDestroyOnLoad(go);

            int index = RegisterGameObject(go, temp);

            return index;
        }

        public static void ClearTemp()
        {
            for (int i = 0, count = goList.Count; i < count; i++)
            {
                if (goActive[i] == ACTIVE)
                {
                    if (goList[i].temp)
                    {
                        indexIdle.Enqueue(i);
                        goActive[i] = DEACTIVE;
                    }
                }
            }
        }

        public static Transform GetTransform(int id)
        {
            if (id < 0 || id >= goActive.Count)
                return null;

            if (goActive[id] == DEACTIVE)
                return null;

            return goList[id].trans;
        }

        public static GameObject GetGameObject(int id)
        {
            if (id < 0 || id >= goActive.Count)
                return null;

            if (goActive[id] == DEACTIVE)
                return null;

            return goList[id].go;
        }

        private static bool CheckID(int id)
        {
            if (id < 0 || id >= goActive.Count)
                return false;

            if (goActive[id] == DEACTIVE)
                return false;

            return true;
        }

        public static int SetParent(int parent, int child, bool reset)
        {
            Transform ptrans = GetTransform(parent);
            if (!ptrans)
                return -1;

            Transform ctrans = GetTransform(child);
            if (!ctrans)
                return -2;

            ctrans.parent = ptrans;

            if (reset)
                GameCommon.ResetTrans(ctrans);

            return 0;
        }

        public static void RemoveParent(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            trans.parent = null;
        }

        public static float TransformSqrMagnitudeDistance(int id, int tid, bool ignoreY)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return 9999999;

            Transform trans1 = GetTransform(tid);
            if (!trans1)
                return 9999999;

            if (ignoreY)
            {
                Vector3 p = trans1.position;
                p.y = trans.position.y;
                return Vector3.SqrMagnitude(trans.position - p);
            }
            else
                return Vector3.SqrMagnitude(trans.position - trans1.position);
        }

        public static float TransformSqrMagnitudeDistance(int id, float x, float y, float z, bool ignoreY)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return 999999999;

            if (ignoreY)
                y = trans.position.y;

            return Vector3.SqrMagnitude(trans.position - new Vector3(x, y, z));
        }

        public static float TransformDistance(int id, float x, float y, float z, bool ignoreY)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return 999999999;

            if (ignoreY)
                y = trans.position.y;

            return Vector3.Distance(trans.position, new Vector3(x, y, z));
        }

        public static float TransformDistance(int id, int tid, bool ignoreY)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return 9999999;

            Transform trans1 = GetTransform(tid);
            if (!trans1)
                return 9999999;

            if (ignoreY)
            {
                Vector3 p = trans1.position;
                p.y = trans.position.y;
                return Vector3.Distance(trans.position, p);
            }
            else
                return Vector3.Distance(trans.position, trans1.position);
        }

        public static int TransformFind(int id, string path, bool temp)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return -1;

            Transform t = trans.Find(path);
            if (!t)
                return -2;

            return RegisterTransform(t, temp);
        }

        private static Transform ErgodicTrans(Transform trans, string name)
        {
            Transform temp, find = null;
            for (int i = 0, count = trans.childCount; i < count; i++)
            {
                temp = trans.GetChild(i);
                if (temp.name == name)
                    return temp;

                find = ErgodicTrans(temp, name);
                if (find != null)
                    break;
            }

            return find;
        }

        public static Transform TransformErgodicFind(int id, string name)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return null;

            return ErgodicTrans(trans, name);
        }

        public static Transform TransformFindChild(int id, string name)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return null;

            return trans.Find(name);
        }

        public static Transform TransformFind(int id, string path)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return null;

            return trans.Find(path);
        }

        public static void SetPosition(int id, float x, float y, float z, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Vector3 pos = trans.position;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
            }
            else
                pos.Set(x, y, z);

            trans.position = pos;
        }

        public static Vector3 GetPosition(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.zero;

            return trans.position;
        }

        public static Vector3 GetForward(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.forward;

            return trans.forward;
        }

        public static void SetForward(int id, float x, float y, float z)
        {
            Transform trans = GetTransform(id);
            if (trans)
                trans.forward = new Vector3(x, y, z);
        }

        public static void SetLocalPosition(int id, float x, float y, float z, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Vector3 pos = trans.localPosition;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
            }
            else
                pos.Set(x, y, z);

            trans.localPosition = pos;
        }

        public static Vector3 GetLocalPosition(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.zero;

            return trans.localPosition;
        }

        public static void SetEulerAngle(int id, float x, float y, float z, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans) 
                return;

            Vector3 pos = trans.eulerAngles;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
            }
            else
                pos.Set(x, y, z);

            trans.eulerAngles = pos;
        }

        public static Vector3 GetEulerAngle(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.zero;

            return trans.eulerAngles;
        }

        public static void SetLocalEulerAngle(int id, float x, float y, float z, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Vector3 pos = trans.localEulerAngles;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
            }
            else
                pos.Set(x, y, z);

            trans.localEulerAngles = pos;
        }

        public static Vector3 GetLocalEulerAngle(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.zero;

            return trans.localEulerAngles;
        }

        public static void SetRotation(int id, float x, float y, float z, float w, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Quaternion pos = trans.rotation;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
                pos.w += w;
            }
            else
                pos.Set(x, y, z, w);

            trans.rotation = pos;
        }

        public static Quaternion GetRotation(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Quaternion.identity;

            return trans.rotation;
        }

        public static void SetLocalRotation(int id, float x, float y, float z, float w, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Quaternion pos = trans.localRotation;
            if (append)
            {
                pos.x += x;
                pos.y += y;
                pos.z += z;
                pos.w += w;
            }
            else
                pos.Set(x, y, z, w);

            trans.rotation = pos;
        }

        public static Quaternion GetLocalRotation(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Quaternion.identity;

            return trans.localRotation;
        }

        public static void SetLocalScale(int id, float x, float y, float z, bool append)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Vector3 scale = trans.localScale;
            if (append)
            {
                scale.x += x;
                scale.y += y;
                scale.z += z;
            }
            else
                scale.Set(x, y, z);

            trans.localScale = scale;
        }

        public static Vector3 GetLocalScale(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return Vector3.one;

            return trans.localScale;
        }

        private static void ErgodicTransformSetLayer(Transform parent, int layer)
        {
            if (parent == null)
                return;
            if (parent.childCount > 0)
            {
                foreach (Transform t in parent)
                {
                    t.gameObject.layer = layer;
                    ErgodicTransformSetLayer(t, layer);
                }
            }
        }

        public static void SetLayer(int id, string layer, bool child)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            go.layer = LayerMask.NameToLayer(layer);
            if (child)
                ErgodicTransformSetLayer(go.transform, go.layer);
        }

        public static void SetLayer(int id, int layer, bool child)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            go.layer = layer;
            if(child)
                ErgodicTransformSetLayer(go.transform, layer);
        }

        public static void SetActive(int id, bool active)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            go.SetActive(active);
        }

        public static bool GetActive(int id)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return false;

            return go.activeSelf;
        }

        public static int GetLayer(int id)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return -1;

            return go.layer;
        }

        public static Component AddComponent(int id, System.Type type)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.AddComponent(type);
        }

        public static Component GetComponent(int id, System.Type type)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.GetComponent(type);
        }

        public static Component GetComponent(int id, System.Type type, string path)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return null;

            Transform t = trans.Find(path);
            if (!t)
                return null;

            return t.GetComponent(type);
        }

        public static Component GetComponentInChildren(int id, System.Type type)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.GetComponentInChildren(type);
        }

        public static Component[] GetComponentsInChildren(int id, System.Type type)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.GetComponentsInChildren(type);
        }

        public static void TransformLookAt(int id, int target)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Transform t = GetTransform(target);
            if (!t)
                return;

            trans.LookAt(t);
        }

        public static void TransformLookAt(int id, int target, float upx, float upy, float upz)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            Transform t = GetTransform(target);
            if (!t)
                return;

            trans.LookAt(t, new Vector3(upx, upy, upz));
        }

        public static void TransformLookAt(int id, float x, float y, float z)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            trans.LookAt(new Vector3(x, y, z));
        }

        public static void TransformLookAt(int id, float x, float y, float z, float upx, float upy, float upz)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            trans.LookAt(new Vector3(x, y, z), new Vector3(upx, upy, upz));
        }

        public static void SetName(int id, string name)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            go.name = name;
        }

        public static string GetName(int id)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.name;
        }

        public static void SetTag(int id, string tag)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            go.tag = tag;
        }

        public static string GetTag(int id)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return null;

            return go.tag;
        }

        public static void SetToStatic(int id)
        {
            GameObject go = GetGameObject(id);
            if (!go)
                return;

            GoObj obj = goList[id];
            obj.temp = false;
            goList[id] = obj;

            Object.DontDestroyOnLoad(go);
        }

        public static void ResetTrans(int id)
        {
            Transform trans = GetTransform(id);
            if (!trans)
                return;

            GameCommon.ResetTrans(trans);
        }

        public static void StoreSkinnedMeshRenderer(int id, bool force)
        {
            if (!CheckID(id))
                return;

            Transform trans = GetTransform(id);
            if (!trans)
                return;

            GoObj obj = goList[id];
            if (obj.smrArr == null || force)
            {
                SkinnedMeshRenderer[] arr = obj.go.GetComponentsInChildren<SkinnedMeshRenderer>();
                obj.smrArr = arr;
                goList[id] = obj;
            }
        }

        public static void StoreMeshRenderer(int id, bool force)
        {
            if (!CheckID(id))
                return;

            Transform trans = GetTransform(id);
            if (!trans)
                return;

            GoObj obj = goList[id];
            if (obj.rendererArr == null || force)
            {
                MeshRenderer[] arr = obj.go.GetComponentsInChildren<MeshRenderer>();
                obj.rendererArr = arr;
                goList[id] = obj;
            }
        }

        public static void SetReceiveShadow(int id, bool recvShadow)
        {
            if(!CheckID(id))
                return;

            Transform trans = GetTransform(id);
            if (!trans)
                return;

            GoObj obj = goList[id];
            if (obj.rendererArr != null)
            {
                for (int i = 0, count = obj.rendererArr.Length; i < count; i++)
                    obj.rendererArr[i].receiveShadows = recvShadow;
            }
            else if (obj.smrArr != null)
            {
                for (int i = 0, count = obj.smrArr.Length; i < count; i++)
                    obj.smrArr[i].receiveShadows = recvShadow;
            }
        }

        // 0:off 1:on 2:two side 3:shadow only
        public static void SetCastShadow(int id, int castShadow)
        {
            if(!CheckID(id))
                return;
            if (castShadow < 0 || castShadow > 3)
                return;

            Transform trans = GetTransform(id);
            if (!trans)
                return;

            UnityEngine.Rendering.ShadowCastingMode mode = (UnityEngine.Rendering.ShadowCastingMode)castShadow;

            GoObj obj = goList[id];
            if (obj.rendererArr != null)
            {
                for (int i = 0, count = obj.rendererArr.Length; i < count; i++)
                    obj.rendererArr[i].shadowCastingMode = mode;
            }
            else if (obj.smrArr != null)
            {
                for (int i = 0, count = obj.smrArr.Length; i < count; i++)
                    obj.smrArr[i].shadowCastingMode = mode;
            }
        }

        public static void SetRendererShow(int id, bool show)
        {
            if (!CheckID(id))
                return;

            Transform trans = GetTransform(id);
            if (!trans)
                return;

            GoObj obj = goList[id];
            if (obj.rendererArr != null)
            {
                for (int i = 0, count = obj.rendererArr.Length; i < count; i++)
                    obj.rendererArr[i].enabled = show;
            }
            else if (obj.smrArr != null)
            {
                for (int i = 0, count = obj.smrArr.Length; i < count; i++)
                    obj.smrArr[i].enabled = show;
            }
        }

        public static int GameObjectFindAndReg(string name, bool temp)
        {
            GameObject go = GameObject.Find(name);
            return RegisterGameObject(go, temp);
        }

        public static GameObject GameObjectFind(string name)
        {
            return GameObject.Find(name);
        }
    }
}
