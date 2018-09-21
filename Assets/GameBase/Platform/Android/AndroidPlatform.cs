

using UnityEngine;
using System.Collections.Generic;

namespace GameBase
{
    public static class AndroidPlatform
    {
#if UNITY_ANDROID
        private static AndroidJavaClass androidPlayer = null;
        private static AndroidJavaObject context = null;
        private static AndroidJavaObject assetMgr = null;

        public static void Init()
        {
            androidPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            context = androidPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            assetMgr = context.Call<AndroidJavaObject>("getAssets");
        }

        public static AndroidJavaClass GetUnityPlayer()
        {
            return androidPlayer;
        }

        public static AndroidJavaObject GetContext()
        {
            return context;
        }

        public static AndroidJavaObject GetAssetMgr()
        {
            return assetMgr;
        }
#else
        public static void Init()
        {
        }
#endif
    }
}
