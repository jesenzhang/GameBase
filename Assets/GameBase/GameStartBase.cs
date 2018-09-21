using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBase
{
    public class GameStartBase : MonoBehaviour
    {
        private static bool inited = false;

        void Start()
        {
            if (inited)
                return;
            inited = true;

            GameCommon.Init();

            ThreadTask.RunAsync(() => { }, null);

            Object.DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;

            BeforeInit();

            Init();
        }

        private void BeforeInit()
        {
            //platform
            Debug.LogError("game start before init");
            if (Application.platform == RuntimePlatform.Android)
            {
                Debug.LogError("game start before init->android platform init");
                AndroidPlatform.Init();
                Debug.LogError("game start before init->android asset file stream init");
                AndroidAssetFileStream.Init();
            }

            gameObject.AddComponent<ResUpdate>();

            GameObject go = new GameObject();
            go.AddComponent<WwwWorkpool>();
            Object.DontDestroyOnLoad(go);

            gameObject.AddComponent<ResLoader>();
            gameObject.AddComponent<GameSceneManager>();
            gameObject.AddComponent<GameTime>();

            LuaLoader.GetInstance();
        }

        protected virtual void Init() { }

        void OnApplicationFocus()
        {
            MessagePool.ScriptSendMessage("", MessagePool.OnApplicationFocus, Message.FilterTypeNothing, "OnApplicationFocus");
        }

        void OnApplicationPause()
        {
            MessagePool.ScriptSendMessage("", MessagePool.OnApplicationPause, Message.FilterTypeNothing, "OnApplicationPause");
        }

        void OnApplicationQuit()
        {
            NetworkManager.CloseAll();
            LuaCThread.CloseAll();
            LuaManager.Dispose();
            LuaContext.DisposeAll();
            LuaLoader.GetInstance().Clean();
            MessagePool.ScriptSendMessage("", MessagePool.OnApplicationQuit, Message.FilterTypeNothing, "OnApplicationQuit");
        }
    }
}
