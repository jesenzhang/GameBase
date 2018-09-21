
using System;
using System.Collections.Generic;
using LuaInterface;
using System.IO;

namespace GameBase
{
    public class LuaContext
    {
        private LuaState lua;

        private static List<LuaContext> contexts = new List<LuaContext>();


        public static LuaContext Create(Action<LuaState> call)
        {
            return new LuaContext(call);
        }

        public static void RefreshDelegateMap()
        {
            LuaContext context;
            for (int i = 0, count = contexts.Count; i < count; i++)
            {
                context = contexts[i];
                if (context != null)
                {
                    context.lua.RefreshDelegateMap();
                }
            }
        }

        public static void DisposeAll()
        {
            LuaContext context;
            for (int i = 0, count = contexts.Count; i < count; i++)
            {
                context = contexts[i];
                if (context != null)
                {
                    context.Dispose();
                }
            }

            contexts.Clear();
        }

        private LuaContext(Action<LuaState> call)
        {
            contexts.Add(this);
            lua = new LuaInterface.LuaState();

            if(call != null)
                call(lua);
            LoadLibs();

            lua.Start();
        }

        public LuaState GetLuaState()
        {
            return lua;
        }

        public void RegisterLibs()
        {
            if (lua == null)
                return;
            lua.OpenLibs(_RegisterLibs);
        }

        private int _RegisterLibs(IntPtr L)
        {
            if (L == null)
                return -1;

            LuaDLL.tolua_pushcfunction(L, GlobalPrint);
            LuaDLL.lua_setglobal(L, "global_print");

            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        private static int GlobalPrint(IntPtr L)
        {
            try
            {
                int n = LuaDLL.lua_gettop(L);
                //StringBuilder sb = StringBuilderCache.Acquire();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
#if UNITY_EDITOR2
                int line = LuaDLL.tolua_where(L, 1);
                string filename = LuaDLL.lua_tostring(L, -1);
                LuaDLL.lua_settop(L, n);

                if (!filename.Contains("."))
                {
                    sb.AppendFormat("[{0}.lua:{1}]:", filename, line);
                }
                else
                {
                    sb.AppendFormat("[{0}:{1}]:", filename, line);
                }
#endif

                for (int i = 1; i <= n; i++)
                {
                    if (i > 1) sb.Append("    ");

                    if (LuaDLL.lua_isstring(L, i) == 1)
                    {
                        sb.Append(LuaDLL.lua_tostring(L, i));
                    }
                    else if (LuaDLL.lua_isnil(L, i))
                    {
                        sb.Append("nil");
                    }
                    else if (LuaDLL.lua_isboolean(L, i))
                    {
                        sb.Append(LuaDLL.lua_toboolean(L, i) ? "true" : "false");
                    }
                    else
                    {
                        IntPtr p = LuaDLL.lua_topointer(L, i);

                        if (p == IntPtr.Zero)
                        {
                            sb.Append("nil");
                        }
                        else
                        {
                            sb.AppendFormat("{0}:0x{1}", LuaDLL.luaL_typename(L, i), p.ToString("X"));
                        }
                    }
                }

                GameBase.Debugger.Log(sb.ToString());
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        public int LuaUpdate(float deltaTime, float unscaleDeltaTime)
        {
            return lua.LuaUpdate(deltaTime, unscaleDeltaTime);
        }

        public void LuaPop(int amount)
        {
            lua.LuaPop(amount);
        }

        public void Collect()
        {
            lua.Collect();
        }

        public bool CheckTop()
        {
            return lua.CheckTop();
        }

        public void Require(string fileName)
        {
            if (lua == null)
                return;

            lua.Require(fileName);
        }

        public int LuaRequire(string fileName)
        {
            if (lua == null)
                return -1;

            return lua.LuaRequire(fileName);
        }

        public LuaFunction GetFunction(string funcName)
        {
            if (lua == null)
                return null;

            return lua.GetFunction(funcName);
        }

        public LuaTable GetTable(string path)
        {
            if (lua == null)
                return null;

            return lua.GetTable(path);
        }

        public LuaTable GetTable(int reference)
        {
            if (lua == null)
                return null;

            if (reference == -1)
                return null;

            return lua.GetTable(reference);
        }

        private void LoadLibs()
        {
            OpenLibs();
            lua.LuaSetTop(0);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        private static int LuaOpen_Socket_Core(IntPtr L)
        {
            return LuaDLL.luaopen_socket_core(L);
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        private static int LuaOpen_Mime_Core(IntPtr L)
        {
            return LuaDLL.luaopen_mime_core(L);
        }

        private void OpenLuaSocket()
        {
            LuaConst.openLuaSocket = true;

            lua.BeginPreLoad();
            lua.RegFunction("socket.core", LuaOpen_Socket_Core);
            lua.RegFunction("mime.core", LuaOpen_Mime_Core);
            lua.EndPreLoad();
        }

        public void OpenZbsDebugger(string ip = "localhost")
        {
            if (!Directory.Exists(LuaConst.zbsDir))
            {
                Debugger.LogWarning("ZeroBraneStudio not install or LuaConst.zbsDir not right");
                return;
            }

            if (!LuaConst.openLuaSocket)
            {
                OpenLuaSocket();
            }

            if (!string.IsNullOrEmpty(LuaConst.zbsDir))
            {
                lua.AddSearchPath(LuaConst.zbsDir);
            }

            lua.LuaDoString(string.Format("DebugServerIp = '{0}'", ip));
        }

        private void OpenLibs()
        {
            //lua.OpenLibs(LuaDLL.luaopen_pb);
            ////lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            ////lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
            //lua.OpenLibs(LuaDLL.luaopen_lpeg);
            //lua.OpenLibs(LuaDLL.luaopen_bit);
            //lua.OpenLibs(LuaDLL.luaopen_socket_core);

            OpenCJson();

            lua.OpenLibs(LuaDLL.luaopen_pb);
            lua.OpenLibs(LuaDLL.luaopen_struct);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        luaState.OpenLibs(LuaDLL.luaopen_bit);
#endif

            if (LuaConst.openLuaSocket)
            {
                OpenLuaSocket();
            }

            if (LuaConst.openLuaDebugger)
            {
                OpenZbsDebugger();
            }
        }

        //cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        private void OpenCJson()
        {
            //lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            //lua.OpenLibs(LuaDLL.luaopen_cjson);
            //lua.LuaSetField(-2, "cjson");

            //lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            //lua.LuaSetField(-2, "cjson.safe");

            lua.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.LuaSetField(-2, "cjson");

            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.LuaSetField(-2, "cjson.safe");
        }

        public void LuaGC()
        {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Dispose()
        {
            if (lua != null)
            {
                lua.Dispose();
                lua = null;
            }
        }
    }
}
