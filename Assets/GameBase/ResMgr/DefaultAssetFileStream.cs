using System.Collections.Generic;
using System.IO;

namespace GameBase
{
    public class DefaultAssetFileStream : IResourceFileStream
    {
        private FileStream fileStream = null;

        public int Read(byte[] arr, int offset, int count)
        {
            if (fileStream == null)
                return -1000;

            fileStream.Position = offset;
            return fileStream.Read(arr, 0, count);
        }

        public bool Open(string path)
        {
            fileStream = File.OpenRead(path);
            return fileStream != null;
        }

        public long Length()
        {
            if (fileStream != null)
                return fileStream.Length;
            else
                return 0;
        }

        public void Close()
        {
            if (fileStream == null)
                return;

            fileStream.Close();
            fileStream = null;
        }
    }
}
