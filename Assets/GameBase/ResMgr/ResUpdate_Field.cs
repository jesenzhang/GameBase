using System;
using System.Collections.Generic;
using System.IO;
using Example;

namespace GameBase
{
    public partial class ResUpdate
    {
        class VersionFileData
        {
            internal List<byte> fileLocate;
        }

        class RelationData
        {
            internal int index;
            internal string origin;
            internal string guid;
            internal int size;
            internal bool encrypt;
            internal VersionFile.Type fileType;
        }

        private const string versionFileName = "c1.bytes";
        private const string downloadFileExtension = ".zip";
        private const string checkVersionFileName = "CheckVersion.bytes";
        private const string versionInfoFileName = "VersionInfo.bytes";

        private VersionFileData versionFileData = null;

        private VersionInfo versionInfo = null;
        private byte[] versionInfoData = null;
        private Dictionary<string, RelationData> nameToRelation = new Dictionary<string, RelationData>();
        private VersionForCheck vfc = null;

        private static ResUpdate instance = null;

        private enum FileLocate
        {
            None = 0,
            StreamingAsset = 1,
            Download = 2,
        }

        private static ResLoader.UpdatePBar updatePBar = null;

        private bool over;

        private bool inited = false;
        private bool canUpdate = false;
        private bool updateVersion = false;
        private int totalNum = 0;
        private int curNum = 0;
        private long totalSize = 0;
        private long curSize = 0;
        private long curFileSize = 0;
        private static MemoryStream reMs = new MemoryStream();

        private static Action doUpdateUIOver;
        private static UpdateBar doUpdateBarV;
        private static Action<float> doCheckUF;

        private static string localAssetPath = null;
        private static string remoteAssetPath = null;

        private static VersionInfo streamingAssetVersionInfo = null;

        private static byte readVersionStamp = 0;

        private int checkUpdateFileIndex = 0;

        private static object lock_getpath = new object();

        private static CollectionFiles directlyLoadFiles = null;

        private static List<int> needUpdateList = null;
    }
}
