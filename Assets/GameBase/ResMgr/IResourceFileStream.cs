using System.Collections.Generic;
using System.IO;

namespace GameBase
{
    public interface IResourceFileStream
    {
        int Read(byte[] arr, int offset, int count);
        bool Open(string path);
        long Length();
        void Close();
    }
}
