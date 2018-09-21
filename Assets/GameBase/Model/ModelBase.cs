using System;
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace GameBase.Model
{
    public abstract class ModelBase : MonoBehaviour
    {
        protected Transform m_transform;
        public delegate void EndProcessModel(ModelBase mb, System.Object endProcessParam);
        protected EndProcessModel endProcessModel;
        protected System.Object endProcessParam;
        protected LuaFunction luaEndProcessModel;

        public delegate bool CheckPlayAnimation(string name);
        protected CheckPlayAnimation checkPlayAnimation;
        protected LuaFunction luaCheckPlayAnimation;

        protected Transform body;
        private int bodyID;

        public abstract float GetAnimationLength(string name);
        public abstract void Show(bool v);


        protected virtual void Awake()
        {
            m_transform = transform;
        }

        public void SetEndProcessModel(EndProcessModel call, System.Object callParam)
        {
            endProcessModel = call;
            endProcessParam = callParam;
        }

        public void SetEndProcessModel(LuaFunction call, System.Object callParam)
        {
            luaEndProcessModel = call;
            endProcessParam = callParam;
        }

        public void SetCheckPlayAnimation(CheckPlayAnimation call)
        {
            checkPlayAnimation = call;
        }

        public void SetCheckPlayAnimation(LuaFunction call)
        {
            luaCheckPlayAnimation = call;
        }

        public Transform GetRoot()
        {
            return body;
        }

        public int GetRootID()
        {
            return bodyID;
        }

        public Transform FindTransPoint(string path)
        {
            if (body)
            {
                return body.Find(path);
            }

            return null;
        }

        protected void ProcessEndModelCall(ModelBase mb, int _bodyID)
        {
            if (endProcessModel != null)
                endProcessModel(mb, endProcessParam);
            if (luaEndProcessModel != null)
                LuaManager.CallFunc_VX(luaEndProcessModel, _bodyID, endProcessParam);
        }

        protected virtual void ProcessAnimationModule()
        {
        }

        public virtual void ProcessModel(GameObject obj)
        {
            if (obj == null)
            {
                if (endProcessModel != null)
                    endProcessModel(null, endProcessParam);
                if (luaEndProcessModel != null)
                    LuaManager.CallFunc_VX(luaEndProcessModel, -1, endProcessParam);
                return;
            }

            if (body != null)
            {
                if (body == obj.transform)
                {
                    if (Config.Detail_Debug_Log())
                        Debug.LogWarning("model base process model over: equal body");

                    ProcessEndModelCall(this, bodyID);
                    return;
                }
                LuaObjs.Destroy(bodyID);
                body = null;
                bodyID = -1;
            }

            body = obj.transform;
            bodyID = LuaObjs.RegisterTransform(body, true);

            Vector3 vec = m_transform.position;
            body.parent = m_transform;
            vec.Set(0, 0, 0);
            body.localPosition = vec;
            body.localRotation = Quaternion.identity;
            vec.Set(1, 1, 1);
            body.localScale = vec;
            body.gameObject.layer = gameObject.layer;

            ErgodicTransform(body, gameObject.layer);

            ProcessAnimationModule();

            if (Config.Detail_Debug_Log())
                Debug.LogError("model base process model call->" + (endProcessModel == null));
            if (endProcessModel != null)
                endProcessModel(this, endProcessParam);
            if (luaEndProcessModel != null)
                LuaManager.CallFunc_VX(luaEndProcessModel, bodyID, endProcessParam);
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

        void OnEnable()
        {
        }

        public virtual void Clear()
        {
            if (body != null)
            {
                GameObject.Destroy(body.gameObject);
                body = null;
            }
        }

        void OnDestroy()
        {
            Clear();
        }
    }
}