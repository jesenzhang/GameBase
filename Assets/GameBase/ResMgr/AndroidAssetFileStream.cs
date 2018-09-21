
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GameBase
{
    public class AndroidAssetFileStream : IResourceFileStream
    {
        private System.IntPtr fileStream = System.IntPtr.Zero;

#if UNITY_ANDROID
        [DllImport("android_util")]
        private static extern int android_asset_init(System.IntPtr mgr);

        [DllImport("android_util")]
        private static extern System.IntPtr android_asset_open(string fileName, int mode);

        [DllImport("android_util")]
        private static extern int android_asset_read(System.IntPtr asset, byte[] buf, int bufsize);

        [DllImport("android_util")]
        private static extern void android_asset_close(System.IntPtr asset);

        [DllImport("android_util")]
        private static extern void android_asset_seek(System.IntPtr asset, int offset, int set);

        [DllImport("android_util")]
        private static extern int android_asset_length(System.IntPtr asset);
#endif

        private const int SEEK_SET = 0;
        private const int SEEK_CUR = 0;
        private const int SEEK_END = 0;


        public static void Init()
        {
#if UNITY_ANDROID
            AndroidJavaObject assetMgr = AndroidPlatform.GetAssetMgr();
            if (assetMgr == null)
            {
                Debug.LogError("get android asset manager");
                return;
            }

            if(Config.Detail_Debug_Log())
                Debug.LogError("begin android asset file stream init");
            int re = android_asset_init(assetMgr.GetRawObject());
            if(Config.Detail_Debug_Log())
                Debug.LogError("android asset file stream init re->" + re);
#endif
        }

        public bool Open(string path)
        {
#if UNITY_ANDROID
            fileStream = android_asset_open(path, 1);
            return fileStream != System.IntPtr.Zero;
#else
            throw new System.NotImplementedException();
#endif
        }

        public int Read(byte[] arr, int offset, int count)
        {
#if UNITY_ANDROID
            if (fileStream == System.IntPtr.Zero)
                return -1000;

            if(offset != 0)
                android_asset_seek(fileStream, offset, SEEK_SET);
            return android_asset_read(fileStream, arr, arr.Length);
#else
            throw new System.NotImplementedException();
#endif
        }

        public long Length()
        {
#if UNITY_ANDROID
            if (fileStream == System.IntPtr.Zero)
                return 0;

            return android_asset_length(fileStream);
#else
            throw new System.NotImplementedException();
#endif
        }

        public void Close()
        {
#if UNITY_ANDROID
            if (fileStream != System.IntPtr.Zero)
            {
                android_asset_close(fileStream);
                fileStream = System.IntPtr.Zero;
            }
#endif
        }
    }
}
