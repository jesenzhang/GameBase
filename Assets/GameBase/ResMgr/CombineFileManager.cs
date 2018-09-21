using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public class CombineFileManager
    {
        private Dictionary<string, CombineFile> combineFileDic = new Dictionary<string, CombineFile>();

        private static CombineFileManager instance;



        private CombineFileManager()
        {
        }

        public CombineFile GetCombineFile(string name)
        {
            CombineFile cf;
            if (!combineFileDic.TryGetValue(name, out cf))
            {
                cf = new CombineFile(name);
                combineFileDic.Add(name, cf);
            }

            return cf;
        }

        public static CombineFileManager GetInstance()
        {
            if (instance == null)
            {
                instance = new CombineFileManager();
            }

            return instance;
        }
    }
}
