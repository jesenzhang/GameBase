using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace GameBase
{
    public partial class ResLoader
    {
        private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

        private static ResLoader instance = null;

        public delegate void EndDownload(string url, byte[] data, System.Object ud);
        public delegate void UpdatePBar(float v);

        private Dictionary<UnityWebRequest, UpdatePBar> updatePBarDic = new Dictionary<UnityWebRequest, UpdatePBar>();

        public delegate void HelpLoadCallback(AssetBundleRequest abr, System.Object param);
    }
}
