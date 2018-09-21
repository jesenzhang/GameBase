using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBase
{
    public class EditorResourceLoader : IPlatformResourceLoader
    {
        public void LoadBundle(string originName, string destName, string path, Example.VersionFile.Type fileType, ResourceLoader.EndLoadBundle endLoad, object obj, bool assetBundle = false)
        {
            throw new NotImplementedException();
        }

        public IResourceFileStream LoadFile(string path)
        {
            throw new NotImplementedException();
        }

        public byte[] SyncReadBytes(string path)
        {
            throw new NotImplementedException();
        }

        public int SyncReadBytes(string path, int begin, int length, byte[] destBuf)
        {
            throw new NotImplementedException();
        }
    }
}
