using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using UnityEngine;

namespace GameBase
{
    public static partial class LuaManager
    {
        public static void CallFunc_VX(LuaFunction func)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, UIFrame ui)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(ui);
            func.PCall();
            func.EndPCall();
        }

        public static void CallFunc_V(LuaFunction func, int a, int b)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(a);
            func.Push(b);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, bool a, float b)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(a);
            func.Push(b);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, bool a, float b, int c)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(a);
            func.Push(b);
            func.Push(c);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, object o, object[] v)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(o);
            func.Push(v);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, object[] v)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(v);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, string v)
        {
            if (func == null)
                return;

            func.BeginPCall();
            func.Push(v);
            func.PCall();
            func.EndPCall();
        }

        public static void CallFunc_V(LuaFunction func, byte[] buf)
        {
            if (func == null)
                return;

            func.BeginPCall();
            if (buf != null)
            {
                LuaByteBuffer luaBuf = new LuaByteBuffer(buf);
                func.Push(luaBuf);
            }
            else
            {
                func.PushObject(null);
            }

            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }
        
        public static void CallFunc_V(LuaFunction func, byte[] buf, object v)
        {
            if (func == null)
                return;

            func.BeginPCall();
            if (buf != null)
            {
                LuaByteBuffer luaBuf = new LuaByteBuffer(buf);
                func.Push(luaBuf);
            }
            else
            {
                func.PushObject(null);
            }
            func.Push(v);
            func.PCall();
            func.EndPCall();
        }

        public static void CallFunc_V(LuaFunction func, byte a, byte b, byte c, byte[] buf)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(a);
            func.Push(b);
            func.Push(c);
            if (buf != null)
            {
                LuaByteBuffer luaBuf = new LuaByteBuffer(buf);
                func.Push(luaBuf);
            }
            else
            {
                func.PushObject(null);
            }
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, byte a, byte b, byte c, byte[] buf, int len)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(a);
            func.Push(b);
            func.Push(c);
            if (buf != null)
            {
                LuaByteBuffer luaBuf = new LuaByteBuffer(buf, len);
                func.Push(luaBuf);
            }
            else
            {
                func.PushObject(null);
            }
            func.PCall();
            func.EndPCall();
        }

        public static void CallFunc_V(LuaFunction func, GameObject go, int v)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(go);
            func.Push(v);
            func.PCall();
            //func.CheckNumber();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, GameObject go, bool v, int iv)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(go);
            func.Push(v);
            func.Push(iv);
            //func.Push(str1);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, GameObject go, float v, int iv)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(go);
            func.Push(v);
            func.Push(iv);
            //func.Push(str1);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, GameObject go, Vector2 v, int iv)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(go);
            func.Push(v);
            func.Push(iv);
            //func.Push(str1);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_V(LuaFunction func, GameObject go, KeyCode v, int iv)
        {
            if (func == null)
                return;
            func.BeginPCall();
            func.Push(go);
            func.Push(v);
            func.Push(iv);
            //func.Push(str1);
            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }

        public static void CallFunc_VX(LuaFunction func, params object[] args)
        {
            if (func == null)
                return;

            func.BeginPCall();
            for (int i = 0, count = args.Length; i < count; i++)
                func.Push(args[i]);

            func.PCall();
            func.EndPCall();
            //lua.CheckTop();
        }
    }
}
