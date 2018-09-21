
using System;
using System.Collections.Generic;
using System.IO;

namespace GameBase
{
    public class CombineFile
    {
        private string name;
        private Example.Combinefiles _combinefiles = null;
        private Dictionary<string, Example.Groupcombine> fileDic = null;
        private bool _initHeader = false;

        private int dataOffset = -1;

        private IResourceFileStream fileStream = null;


        public CombineFile(string name)
        {
            this.name = name;
        }

        private void InitHeader()
        {
            if (_initHeader)
                return;
            _initHeader = true;

            IResourceFileStream irs = ResLoader.LoadFileStreamByName(name);
            if (irs == null)
            {
                if (Config.Debug_Log())
                    Debugger.LogError("ERROR: ivnalid combine file name->" + name);
                return;
            }
            byte[] dataArr = new byte[4];
            irs.Read(dataArr, 0, 4);

            MemoryStream ms = new MemoryStream(dataArr);
            BinaryReader br = new BinaryReader(ms);
            int headlen = br.ReadInt32();
            dataOffset = 4 + headlen;

            dataArr = new byte[headlen];
            irs.Read(dataArr, 4, headlen);
            irs.Close();

            ms = new MemoryStream(dataArr);
            _combinefiles = Example.Combinefiles.Deserialize(ms);

            if (_combinefiles != null)
            {
                fileDic = new Dictionary<string, Example.Groupcombine>();
                for (int i = 0, count = _combinefiles.Files.Count; i < count; i++)
                {
                    fileDic.Add(_combinefiles.Files[i].Value, _combinefiles.Groups[i]);
                }

                _combinefiles.Files.Clear();
                _combinefiles.Groups.Clear();
            }
        }

        public void CollectAllFilePath(List<string> lst)
        {
            if (fileDic == null)
                InitHeader();
            if (_combinefiles != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0, count = _combinefiles.Files.Count; i < count; i++)
                {
                    Example.Groupcombine gc = _combinefiles.Groups[i];
                    for (int j = 0, jcount = gc.Infos.Count; j < jcount; j++)
                    {
                        Example.Combineinfo info = gc.Infos[j];
                        sb.Append(_combinefiles.Dirs[info.Dir]);
                        sb.Append(_combinefiles.Files[i]);

                        lst.Add(sb.ToString());
                    }
                }
            }
        }

        public void Reset()
        {
            Close();
            fileDic = null;
            _initHeader = false;
            _combinefiles = null;
        }

        private Example.Combineinfo GetCombineInfo(string path)
        {
            if (fileDic == null)
                InitHeader();
            if (fileDic == null)
                return null;
            string fileName = Path.GetFileName(path);
            Example.Groupcombine group;
            if (!fileDic.TryGetValue(fileName, out group))
            {
                return null;
            }

            Example.Combineinfo info = null;
            if (group.Infos.Count > 1)
            {
                string filePath = GetDirectoryPath(path);
                Example.Combineinfo ci = null;
                string dir = null;
                float simlarValue = -1;
                float tempSimlarValue;
                for (int i = 0, count = group.Infos.Count; i < count; i++)
                {
                    ci = group.Infos[i];
                    if (ci.Dir < 0 || ci.Dir >= _combinefiles.Dirs.Count)
                        continue;

                    dir = _combinefiles.Dirs[ci.Dir].Value;
                    if (IsSimilarPath(dir, filePath, out tempSimlarValue))
                    {
                        if (tempSimlarValue > simlarValue)
                        {
                            info = ci;
                            simlarValue = tempSimlarValue;
                        }
                        else if (simlarValue != 0 && tempSimlarValue == simlarValue)
                        {
                            if (info != null)
                            {
                                Debugger.LogError("asset error: check is simplar path has same simlar value->" + _combinefiles.Dirs[info.Dir].Value + "^" + dir + "^" + filePath + "^" + fileName + "^" + path);
                                Debugger.LogError("------simlar path count->" + group.Infos.Count);
                                for (int j = 0, jcount = group.Infos.Count; j < jcount; j++)
                                {
                                    ci = group.Infos[j];
                                    if (ci.Dir < 0 || ci.Dir >= _combinefiles.Dirs.Count)
                                        continue;
                                    Debugger.LogError("------simlar path->" + j + "^" + _combinefiles.Dirs[ci.Dir].Value);
                                }
                            }
                            else
                                Debugger.LogError("asset error: check is simplar path has same simlar value info is null->" + dir + "^" + filePath + "^" + fileName + "^" + path);
                        }
                    }
                }
            }
            else if (group.Infos.Count == 1)
            {
                info = group.Infos[0];
            }

            if (info == null)
            {
                if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
                {
                    Example.Groupcombine gcf;
                    if (fileDic.TryGetValue(fileName, out gcf))
                    {
                        string str = null;
                        for (int i = 0, count = gcf.Infos.Count; i < count; i++)
                        {
                            str += _combinefiles.Dirs[gcf.Infos[i].Dir].Value + "^";
                        }

                        UnityEngine.Debug.LogError("combine exception file->" + path + "^" + fileName + ": " + str);
                    }
                }
                return null;
            }

            return info;
        }

        private int GetRealOffset(Example.Combineinfo info)
        {
            return dataOffset + info.Start;
        }

        public void GetFileDetail(string path, out int offset, out int size, out bool encrypt)
        {
            offset = -1;
            size = 0;
            encrypt = false;
            Example.Combineinfo info = GetCombineInfo(path);
            if (info == null)
                return;

            offset = GetRealOffset(info);
            size = info.Size;
            encrypt = info.Encrypt;
        }

        private bool IsSimilarPath(string origin, string dest, out float simlarValue)
        {
            string p1 = origin;
            string p2 = dest;
            simlarValue = 0;
            if (p1 == null || p2 == null)
                return false;
            if (p1.Length == 0 && p2.Length == 0)
                return true;

            if (p1.Length == 0 || p2.Length == 0)
            {
                return false;
            }

            string longStr = null;
            string shortStr = null;
            if (p1.Length > p2.Length)
            {
                longStr = p1;
                shortStr = p2;
            }
            else
            {
                longStr = p2;
                shortStr = p1;
            }

            char sc, lc;
            char vsc = '♪';
            int vscIndex = 0;
            for (int i = 0, count = shortStr.Length; i < count; i++)
            {
                sc = shortStr[i];
                if (sc == '.' || sc == '/' || sc == '\\')
                    continue;
                vsc = sc;
                vscIndex = i;
                break;
            }

            int longStrLen = longStr.Length;
            int shortStrLen = shortStr.Length;
            int num = 0;
            for (int i = shortStrLen - 1; i >= vscIndex; i--)
            {
                sc = shortStr[i];
                lc = longStr[longStrLen - 1 - (shortStrLen - 1 - i)];
                if (sc != lc)
                {
                    if ((sc == '/' || sc == '\\') && (lc == '/' || lc == '\\'))
                        continue;
                    return false;
                }

                num++;
            }

            simlarValue = (float)num / dest.Length;
            simlarValue += (float)shortStr.Length / longStr.Length * 100;

            return true;
        }

        public bool Open()
        {
            if (fileStream != null)
                return true;
            fileStream = ResLoader.LoadFileStreamByName(name);
            return fileStream != null;
        }

        public void Close()
        {
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        private string GetDirectoryPath(string path)
        {
            if (path == null)
                return null;

            int index = 0;
            char sc = '♪';
            for (int i = path.Length - 1; i >= 0; i--)
            {
                sc = path[i];
                if (sc == '/' || sc == '\\')
                {
                    index = i;
                    break;
                }
            }

            if (index > 0)
                return path.Substring(0, index);
            else
                return "";
        }

        public byte[] Read(string path)
        {
            Example.Combineinfo info = GetCombineInfo(path);
            if (info == null)
                return null;

            if (info.Size <= 0)
                return null;

            if (fileStream != null)
            {
                byte[] data = new byte[info.Size];
                int offset = GetRealOffset(info);
                fileStream.Read(data, offset, info.Size);
                if (info.Encrypt)
                    ResLoader.RemoveImpurity(data, null, false);

                return data;
            }
            else
            {
                IResourceFileStream fs = ResLoader.LoadFileStreamByName(name);
                if (fs == null)
                    return null;

                byte[] data = new byte[info.Size];
                int offset = GetRealOffset(info);
                fs.Read(data, offset, info.Size);
                if (info.Encrypt)
                    ResLoader.RemoveImpurity(data, null, false);

                return data;
            }
        }
    }
}
