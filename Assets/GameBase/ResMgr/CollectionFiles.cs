using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Example;

namespace GameBase
{
    public class CollectionFiles
    {
        class RelationData
        {
            internal string origin;
            internal string dest;
            internal string path;
            internal int size;
            internal bool encrypt;
            internal VersionFile.Type fileType;
        }

        private Dictionary<string, RelationData> files = new Dictionary<string, RelationData>();
        private string rootPath = null;

        public CollectionFiles(string root)
        {
            rootPath = root;
        }

        public void Init()
        {
            Collect(rootPath);
        }

        private void Collect(string root)
        {
            string[] paths = Directory.GetFiles(root);
            if (paths != null)
            {
                string fileName = null;
                for (int i = 0, count = paths.Length; i < count; i++)
                {
                    fileName = Path.GetFileName(paths[i]);
                    if (files.ContainsKey(fileName))
                    {
                        Debug.LogError("duplicate files->" + files[fileName] + "^" + paths[i]);
                        continue;
                    }

                    if (fileName == null || fileName == "")
                        continue;

                    string path = paths[i];
                    string suffix = Path.GetExtension(path);
                    if (suffix == ".cf")
                    {
                        RelationData rd = new RelationData();
                        rd.origin = fileName;
                        rd.dest = fileName;
                        rd.path = path;
                        rd.encrypt = false;
                        rd.fileType = VersionFile.Type.COMBINE_FILE;

                        files.Add(fileName, rd);

                        CombineFile cf = new CombineFile(fileName);
                        List<string> lst = new List<string>();
                        cf.CollectAllFilePath(lst);
                        for (int a = 0, acount = lst.Count; a < acount; a++)
                        {
                            files.Add(lst[a], rd);
                        }
                    }
                    else if (suffix == ".rf")
                    {
                        RelationData rd = new RelationData();
                        rd.origin = fileName;
                        rd.dest = fileName;
                        rd.path = path;
                        rd.encrypt = false;
                        rd.fileType = VersionFile.Type.RELATION_FILE;

                        files.Add(fileName, rd);

                        using (FileStream fs = File.OpenRead(path))
                        {
                            var rf = RelationFile.Deserialize(fs);
                            files.Add(rf.Name, rd);
                        }
                    }
                    else
                    {
                        RelationData rd = new RelationData();
                        rd.origin = fileName;
                        rd.dest = fileName;
                        rd.path = path;
                        rd.encrypt = false;
                        rd.fileType = VersionFile.Type.DEFAULT;

                        files.Add(fileName, rd);
                    }
                }
            }

            string[] dirs = Directory.GetDirectories(root);
            if (dirs != null)
            {
                for (int i = 0, count = dirs.Length; i < count; i++)
                {
                    Collect(dirs[i]);
                }
            }
        }

        public void GetRealPath(string fileName, out string destName, out string path, out bool encrypt, out VersionFile.Type fileType)
        {
            destName = null;
            path = null;
            encrypt = false;
            fileType = VersionFile.Type.DEFAULT;

            RelationData rd;
            if (files.TryGetValue(fileName, out rd))
            {
                destName = rd.dest;
                path = rd.path;
                encrypt = rd.encrypt;
                fileType = rd.fileType;
            }
        }
    }
}
