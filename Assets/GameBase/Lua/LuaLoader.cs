using LuaInterface;
using System.IO;
using UnityEngine;

namespace GameBase
{
    public class LuaLoader : LuaFileUtils
    {
        private object lockobj = new object();

        private CombineFile combineFile = null;
        private long combineFileOpenTime = 0;
        private Timer closeStreamTimer = null;

        private static LuaLoader instance;



        public static LuaLoader GetInstance()
        {
            if (instance == null)
                instance = new LuaLoader();
            return instance;
        }

        //private bool early = false;
        private LuaLoader()
        {
            instance = this;
            beZip = false;
            Debug.LogError("lua loader init->" + Application.platform);
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer)
            {
#if LUA_FILE_COMBINE
                Debug.LogError("lua loader init 1");
                combineFile = new CombineFile("all_lua.cf");
                Timer.CreateTimer(1, -1, DoUpdate, null);
#endif
            }
            else
            {
                Debug.LogError("lua loader init 2");
                AddSearchPath(Application.dataPath + "/Lua/?.lua");
                AddSearchPath(Application.dataPath + "/ToLua/Lua/?.lua");
                //AddSearchPath(Application.dataPath + "/ToLua/Lua/protobuf/?.lua");
                AddSearchPath(Application.dataPath + "/Lua/Proto/?.lua");
            }
        }

        public void Reset()
        {
#if LUA_FILE_COMBINE
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                lock (lockobj)
                {
                    if (combineFile != null)
                        combineFile.Reset();
                }
            }
#endif
        }

        public void Clean()
        {
            Reset();
        }

        /*
        public void SetLoad(bool early)
        {
            this.early = early;
        }
        */

        public override byte[] ReadFile(string fileName)
        {
            lock (lockobj)
            {

                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    /*
                    //Debugger.Log("lua read file 1->" + fileName);
                    //fileName = Path.GetFileName(fileName);
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                    //string path = ResUpdate.GetLoadPath(fileName + ".lua");
                    return ResLoader.SyncReadBytesByName(fileName + ".lua");
                    */

#if LUA_FILE_COMBINE
                    if (combineFile == null)
                    {
                        Debug.LogError("read lua file combine file is null");
                        return null;
                    }
                    if (!combineFile.Open())
                    {
                        Debug.LogError("read lua file combine file open failed");
                        return null;
                    }
                    //combineFileOpenTime = Time.realtimeSinceStartup;
                    combineFileOpenTime = DateTimer.TotalSeconds;
                    if (!fileName.EndsWith(".lua"))
                    {
                        fileName += ".lua";
                    }
                    return combineFile.Read(fileName);
#else
                    fileName = Path.GetFileNameWithoutExtension(fileName);
                    return ResLoader.SyncReadBytesByName(fileName + ".lua");
#endif
                }
                else
                    return base.ReadFile(fileName);
            }
        }

        private void DoUpdate(System.Object param)
        {
            if (combineFileOpenTime > 0)
            {
                //float curTime = Time.realtimeSinceStartup;
                float curTime = DateTimer.TotalSeconds;
                if ((curTime - combineFileOpenTime) > 10)
                {
                    lock (lockobj)
                    {
                        //Debug.LogError("lua reader close combine file");
                        combineFile.Close();
                        combineFileOpenTime = -1;
                    }
                }
            }
        }
    }
}
