using System.Collections.Generic;
using System.Collections;
using UnityEngine;
 

namespace GameBase
{
    public interface IPlatformResourceLoader
    {
        byte[] SyncReadBytes(string path);
        int SyncReadBytes(string path, int begin, int length, byte[] destBuf);

        IResourceFileStream LoadFile(string path);

		void LoadBundle(string originName, string destName, string path, Example.VersionFile.Type fileType, GameBase.ResourceLoader.EndLoadBundle endLoad, System.Object obj, bool assetBundle = false);//, bool cb_whatever = false);
    }
}
