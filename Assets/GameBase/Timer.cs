using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameBase
{
    public class Timer
    {
        private static SecurityQueue<TD> disposeQueue = new SecurityQueue<TD>();

        private static short idx = 0;

        private static LuaInterface.LuaFunction callLuaFunc = null;


        private bool _dispose = false;
        public bool dispose
        {
            get { return _dispose; }
        }

        public void SetDispose()
        {
            _dispose = true;
        }


        private TD _td = null;
        private TD td
        {
            get { return _td; }
            set { _td = value; }
        }

        public static void AddDispose(Timer td, bool cb = false)
        {
            if (td == null)
                return;
            if (td.dispose)
                return;
            if (td.td == null)
                return;

            if (cb)
            {
                if (td.td.doSomething != null)
                    td.td.doSomething(td.td.param);
                else if (td.td.luaDoSth != null)
                    LuaManager.CallFunc_VX(td.td.luaDoSth, td.td.param, 0);
            }

            td.SetDispose();
            if (disposeQueue.Contains(td.td))
                return;

            if (td.td.luaDoSth != null)
                LuaManager.CallFunc_VX(td.td.luaDoSth, td.td.param, 1);

            td.td.doSomething = null;
            td.td.param = null;
            td.td.luaDoSth = null;
            td.td.StopAllCoroutines();
            td.td.gameObject.SetActive(false);
            td.td.timer = null;
            disposeQueue.Enqueue(td.td);
            td.td = null;
        }

        public static void DisposeTimer(TD td)
        {
            if (td == null)
                return;

            AddDispose(td.timer);
        }

        public static Timer LuaCreateTimer(float interval, int count, int param)
        {
            if (interval <= 0)
                return null;

            if (callLuaFunc == null)
                callLuaFunc = LuaManager.GetFunction("TimerMgr.Back");

            if (callLuaFunc == null)
                return null;

            TD td;
            if (disposeQueue.Count > 0)
            {
                disposeQueue.Dequeue(out td);
                if (td != null)
                    td.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = new GameObject();
                Object.DontDestroyOnLoad(go);
                td = go.AddComponent<TD>();
                td.id = ++idx;
            }
            if (td == null)
                return null;

            td.interval = interval;
            td.count = (short)count;
            td.doSomething = null;
            td.luaDoSth = callLuaFunc;
            td.param = param;


            Timer t = new Timer();

            t.td = td;
            td.timer = t;

            td.StartTimer();
            return t;
        }

        public static Timer CreateTimer(float interval, int count, TD.DoSomething dos, System.Object param)
        {
            if (interval <= 0 || dos == null)
                return null;

            TD td;
            if (disposeQueue.Count > 0)
            {
                disposeQueue.Dequeue(out td);
                if (td != null)
                    td.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = new GameObject();
                Object.DontDestroyOnLoad(go);
                td = go.AddComponent<TD>();
                td.id = ++idx;
            }
            if (td == null)
                return null;

            td.interval = interval;
            td.count = (short)count;
            td.doSomething = dos;
            td.param = param;


            Timer t = new Timer();

            t.td = td;
            td.timer = t;

            td.StartTimer();
            return t;
        }
    }

    public class TD : MonoBehaviour
    {
        private float _interval = 0;
        public float interval
        {
            get { return _interval; }
            set { _interval = value; }
        }
        private short _count = 1;
        public short count
        {
            get { return _count; }
            set { _count = value; }
        }

        public Timer timer = null;

        public short id = -1;

        private short cur = 0;

        public delegate void DoSomething(System.Object param);
        public DoSomething doSomething;
        public System.Object param;

        public LuaInterface.LuaFunction luaDoSth;

        internal void StartTimer()
        {
            if (_interval <= 0 || (doSomething == null && luaDoSth == null))
                return;
            cur = 0;
            StartCoroutine(Do());
        }

        private IEnumerator Do()
        {
        DO:
            if (_count > 0)
                cur++;
            yield return new WaitForSeconds(_interval);

            if (timer != null && !timer.dispose)
            {
                if (doSomething != null)
                    doSomething(param);
                else if(luaDoSth != null)
                    LuaManager.CallFunc_VX(luaDoSth, param, 0);

                if (_count == -1)
                    goto DO;
                else
                {
                    if (cur < _count)
                        goto DO;
                }
            }

            Timer.DisposeTimer(this);
        }
    }
}