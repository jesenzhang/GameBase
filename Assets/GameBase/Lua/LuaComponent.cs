
using System.Collections.Generic;
using UnityEngine;
using LuaInterface;

namespace GameBase
{
    public class LuaComponent : MonoBehaviour
    {
        public enum Evt
        {
            Start = 0,
            Update = 1,
            FixedUpdate = 2,
            LateUpdate = 3,
            OnApplicationFocus = 4,
            OnApplicationPause = 5,
            OnApplicationQuit = 6,
            OnBecameInvisible = 7,
            OnBecameVisible = 8,
            OnCollisionEnter = 9,
            OnCollisionExit = 10,
            OnCollisionStay = 11,
            OnDestroy = 12,
            OnDisable = 13,
            OnEnable = 14,
            OnTriggerEnter = 15,
            OnTriggerExit = 16,
            OnTriggerStay = 17,

            //Dont Modify
            COUNT = 18,
        }

        private byte[] evt = new byte[(int)Evt.COUNT];
        private LuaFunction[] funcArr = new LuaFunction[(int)Evt.COUNT];


        public void Register(Evt e, LuaFunction func)
        {
            int v = (int)e;
            evt[v] = 1;
            funcArr[v] = func;
        }

        private void RunEvtFunc(Evt e, params object[] param)
        {
            LuaFunction func = funcArr[(int)e];
            if (func == null)
                return;

            LuaManager.CallFunc_VX(func, param);
        }

        void Start()
        {
            if (evt[(int)Evt.Start] == 0)
                return;

            RunEvtFunc(Evt.Start);
        }

        void Update()
        {
            if (evt[(int)Evt.Update] == 0)
                return;
            RunEvtFunc(Evt.Update);
        }

        void FixedUpdate()
        {
            if (evt[(int)Evt.FixedUpdate] == 0)
                return;
            RunEvtFunc(Evt.FixedUpdate);
        }

        void LateUpdate()
        {
            if (evt[(int)Evt.LateUpdate] == 0)
                return;
            RunEvtFunc(Evt.LateUpdate);
        }

        void OnApplicationFocus(bool v)
        {
            if (evt[(int)Evt.OnApplicationFocus] == 0)
                return;
            RunEvtFunc(Evt.OnApplicationFocus, v);
        }

        void OnApplicationPause(bool v)
        {
            if (evt[(int)Evt.OnApplicationPause] == 0)
                return;
            RunEvtFunc(Evt.OnApplicationPause, v);
        }

        void OnApplicationQuit()
        {
            if (evt[(int)Evt.OnApplicationQuit] == 0)
                return;
            RunEvtFunc(Evt.OnApplicationQuit);
        }

        void OnBecameInvisible()
        {
            if (evt[(int)Evt.OnBecameInvisible] == 0)
                return;
            RunEvtFunc(Evt.OnBecameInvisible);
        }

        void OnBecameVisible()
        {
            if (evt[(int)Evt.OnBecameVisible] == 0)
                return;
            RunEvtFunc(Evt.OnBecameVisible);
        }

        void OnCollisionEnter(Collision col)
        {
            if (evt[(int)Evt.OnCollisionEnter] == 0)
                return;
            RunEvtFunc(Evt.OnCollisionEnter);
        }

        void OnCollisionExit(Collision col)
        {
            if (evt[(int)Evt.OnCollisionExit] == 0)
                return;
            RunEvtFunc(Evt.OnCollisionExit);
        }

        void OnCollisionStay(Collision col)
        {
            if (evt[(int)Evt.OnCollisionStay] == 0)
                return;
            RunEvtFunc(Evt.OnCollisionStay);
        }

        void OnDestroy()
        {
            if (evt[(int)Evt.OnDestroy] == 0)
                return;
            RunEvtFunc(Evt.OnDestroy);

            LuaFunction func;
            for (int i = 0, count = funcArr.Length; i < count; i++)
            {
                func = funcArr[i];
                if (func != null)
                {
                    func.Dispose();
                    funcArr[i] = null;
                    func = null;
                }
            }
        }

        void OnDisable()
        {
            if (evt[(int)Evt.OnDisable] == 0)
                return;
            RunEvtFunc(Evt.OnDisable);
        }

        void OnEnable()
        {
            if (evt[(int)Evt.OnEnable] == 0)
                return;
            RunEvtFunc(Evt.OnEnable);
        }

        void OnTriggerEnter(Collider col)
        {
            if (evt[(int)Evt.OnTriggerEnter] == 0)
                return;
            RunEvtFunc(Evt.OnTriggerEnter);
        }

        void OnTriggerExit(Collider col)
        {
            if (evt[(int)Evt.OnTriggerExit] == 0)
                return;
            RunEvtFunc(Evt.OnTriggerExit);
        }

        void OnTriggerStay(Collider col)
        {
            if (evt[(int)Evt.OnTriggerStay] == 0)
                return;
            RunEvtFunc(Evt.OnTriggerStay);
        }
    }
}
