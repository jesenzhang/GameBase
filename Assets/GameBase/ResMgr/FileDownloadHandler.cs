using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

namespace GameBase
{
    public class FileDownloadHandler : DownloadHandlerScript
    {
        private int expected = -1;
        private int received = 0;
        private string filepath;
        private string originPath;
        private FileStream fileStream;
        private bool canceled = false;
        private int pindex = 0;
        private bool removeImpurity;
        private int originSize;

        public FileDownloadHandler(byte[] buffer, string originPath, int originSize, bool removeImpurity)
          : base(buffer)
        {
            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("file donwload handler init");

            this.originPath = originPath;
            this.removeImpurity = removeImpurity;
            this.originSize = originSize;
            fileStream = LoadAndToTemp.LoadToTempAdditive_Begin(originPath, out filepath);

            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("file donwload handler init over->" + filepath);
        }

        protected override byte[] GetData() { return null; }

        public string GetDataFilePath()
        {
            return filepath;
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("receive data->" + filepath + "^" + dataLength);
            if (expected <= 0)
                expected = originSize;
            if (data == null || data.Length < 1)
            {
                return false;
            }
            received += dataLength;
            if (!canceled)
                LoadAndToTemp.LoadToTempAdditive(filepath, fileStream, data, dataLength, expected, removeImpurity, ref pindex);
            return true;
        }

        protected override float GetProgress()
        {
            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("get progress->" + received + "^" + expected);
            if (expected < 0) return 0;
            return (float)received / expected;
        }

        protected override void CompleteContent()
        {
            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("complete content->" + received + "^" + expected);
            if (fileStream != null)
            {
                LoadAndToTemp.LoadToTempAdditive_End(filepath, fileStream);
                fileStream = null;
            }
        }

        protected override void ReceiveContentLength(int contentLength)
        {
            expected = contentLength;
            if(Config.Detail_Debug_Log())
                UnityEngine.Debug.LogError("receive content length->" + received + "^" + expected);
        }

        public void Close()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        public void Cancel()
        {
            canceled = true;
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
            LoadAndToTemp.RemovePath(originPath);
            File.Delete(filepath);
        }
    }
}
