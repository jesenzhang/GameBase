
using UnityEngine;
using System.Collections.Generic;

namespace GameBase
{
    public static class LuaTweenValue
    {
        class Info
        {
            private int id;
            public GameObject go;
            public LuaInterface.LuaFunction func;
            public int funcParam;
            public bool active;

            public Info(int id)
            {
                this.id = id;
                go = new GameObject();
                go.name = "LuaTween";
                Object.DontDestroyOnLoad(go);
                active = false;
            }

            public int GetID()
            {
                return id;
            }

            public void SetActive(bool v)
            {
                if (go.activeSelf != v)
                    go.SetActive(v);

                active = v;
            }

            public void TweenCallFunc(float factor, bool isFinished)
            {
                if (func != null)
                    LuaManager.CallFunc_VX(func, funcParam, factor, isFinished);

                if (isFinished)
                {
                    func = null;
                    funcParam = -1;
                    Dispose(this);
                }
            }
        }

        private static List<Info> pool = new List<Info>();
        private static Queue<int> idles = new Queue<int>();


        private static TweenValue GenTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            int index = -1;
            if (idles.Count > 0)
            {
                index = idles.Dequeue();
            }

            if (index < 0)
            {
                index = pool.Count;
                Info info = new Info(index);
                pool.Add(info);
            }

            Info cur = pool[index];
            cur.func = func;
            cur.funcParam = param;
            cur.SetActive(true);

            TweenValue tween = TweenValue.Begin<TweenValue>(cur.go, duration);
            tween.SetDelegateFunc(cur.TweenCallFunc);
            return tween;
        }

        public static void GenLinearTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.Linear;
        }

        public static void GenEaseInTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.EaseIn;
        }

        public static void GenEaseOutTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.EaseOut;
        }

        public static void GenEaseInOutTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.EaseInOut;
        }

        public static void GenBounceInTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.BounceIn;
        }

        public static void GenBounceOutTween(float duration, LuaInterface.LuaFunction func, int param)
        {
            TweenValue tween = GenTween(duration, func, param);
            tween.method = UITweener.Method.BounceOut;
        }

        private static void Dispose(Info info)
        {
            if (!info.active)
                return;

            info.SetActive(false);
            idles.Enqueue(info.GetID());
        }
    }
}
