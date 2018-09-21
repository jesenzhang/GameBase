using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaInterface
{
    public static partial class ToLua
    {
        private static void OpenLibs_Ex(IntPtr L)
        {
            LuaDLL.tolua_pushcfunction(L, Print_Error);
            LuaDLL.lua_setglobal(L, "print_error");

            LuaDLL.tolua_pushcfunction(L, Print_Warning);
            LuaDLL.lua_setglobal(L, "print_warning");
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int Print_Error(IntPtr L)
        {
            try
            {
                int n = LuaDLL.lua_gettop(L);
                StringBuilder sb = StringBuilderCache.Acquire();
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

                sb.Append("[");
                sb.Append(DateTime.Now.ToString("HH:mm:ss"));
                sb.Append("] ");

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

                GameBase.Debugger.LogError_Fixed(StringBuilderCache.GetStringAndRelease(sb));
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int Print_Warning(IntPtr L)
        {
            try
            {
                int n = LuaDLL.lua_gettop(L);
                StringBuilder sb = StringBuilderCache.Acquire();
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

                sb.Append("[");
                sb.Append(DateTime.Now.ToString("HH:mm:ss"));
                sb.Append("] ");

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

                GameBase.Debugger.LogWarning_Fixed(StringBuilderCache.GetStringAndRelease(sb));
                return 0;
            }
            catch (Exception e)
            {
                return LuaDLL.toluaL_exception(L, e);
            }
        }
    }
}
