using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;

namespace GameBase
{
    public static partial class LuaManager
    {
        private static LuaContext lua = null;
        private static bool inited = false;

        private static Action<LuaState> registerGameCall;


        internal static Action<LuaState> GetRegisterGameCall()
        {
            return registerGameCall;
        }

        public static LuaContext GetLuaContext()
        {
            return lua;
        }

        public static void Require(string fileName)
        {
            if (lua == null)
            {
                GameBase.Debugger.LogError("need init LuaManager");
                return;
            }

            lua.Require(fileName);
        }

        public static int LuaRequire(string fileName)
        {
            if (lua == null)
            {
                GameBase.Debugger.LogError("need init LuaManager");
                return -1;
            }

            return lua.LuaRequire(fileName);
        }

        public static LuaTable GetTable(string path)
        {
            if (lua == null)
            {
                GameBase.Debugger.LogError("need init LuaManager");
                return null;
            }

            return lua.GetTable(path);
        }

        public static LuaTable GetTable(int reference)
        {
            if (lua == null)
            {
                GameBase.Debugger.LogError("need init LuaManager");
                return null;
            }

            if (reference == -1)
                return null;

            return lua.GetTable(reference);
        }

        public static LuaFunction GetFunction(string funcName)
        {
            if (lua == null)
            {
                GameBase.Debugger.LogError("need init LuaManager");
                return null;
            }

            return lua.GetFunction(funcName);
        }

        public static void Init(Action<LuaState> call)
        {
            if (inited)
                return;
            inited = true;
            registerGameCall = call;
            lua = LuaContext.Create(call);

            LuaStart();
        }


        private static void LuaStart()
        {
            lua.Require("GameStart");

            LuaInterface.LuaFunction func = lua.GetFunction("Start");
            CallFunc_VX(func);
        }

        internal static void LuaGC()
        {
            if (lua == null)
                return;
            lua.LuaGC();
        }

        public static void Dispose()
        {
            if (lua == null)
                return;
            lua = null;
        }
    }
}
