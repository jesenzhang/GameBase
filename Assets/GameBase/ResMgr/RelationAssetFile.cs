using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Example;
using System.IO;

namespace GameBase
{
    public class RelationAssetFile
    {
        struct LoadData
        {
            internal string name;
        }

        private string originName;
        private RelationFile relationFile = null;

        private List<string> loadedList = new List<string>();

        private UnityEngine.Object mainAsset = null;

        private bool loadOver = false;

        class LayerLoadParam
        {
            internal int layer;
            internal int num;
        }

        struct LoadInfo
        {
            internal string name;
            internal LayerLoadParam param;
        }



        public RelationAssetFile(string name)
        {
            originName = name;
        }

        public UnityEngine.Object GetMainAsset()
        {
            return mainAsset;
        }

        public bool Over()
        {
            return loadOver;
        }

        private void SetOver()
        {
            loadOver = true;
        }

        public void Load()
        {
            loadOver = false;
            UnLoad();
            ResLoader.AsynReadBytesByName(originName, EndReadBytes, null, true);
        }

        private void EndReadBytes(byte[] data, object obj)
        {
            if (data == null)
            {
                SetOver();
                return;
            }

            MemoryStream ms = new MemoryStream(data);
            relationFile = RelationFile.Deserialize(ms);
            if (relationFile != null)
            {
                CoroutineHelper.CreateCoroutineHelper(LoadAsset(relationFile));
            }
            else
            {
                SetOver();
            }
        }

        private IEnumerator LoadAsset(RelationFile rf)
        {
            if (rf.Nodes.Count == 0)
            {
                SetOver();
                yield break;
            }

            var node = rf.Nodes[0];

            if (node.Nodes.Count() > 0)
            {
                LayerLoadParam layerParam = new LayerLoadParam();
                layerParam.layer = 0;
                layerParam.num = 0;


                for (int i = 0, count = node.Nodes.Count; i < count; i++)
                {
                    CoroutineHelper.CreateCoroutineHelper(LoadBundle(node.Nodes[i], layerParam));
                }

                while (layerParam.num >= 0 && layerParam.num < node.Nodes.Count())
                {
                    yield return null;
                }

                if (layerParam.num < 0)
                {
                    UnLoad();
                    SetOver();
                    yield break;
                }
            }

            ResLoader.LoadByName(node.File, EndLoadAsset, new LoadInfo() { name = node.File, param = null});
        }

        private void EndLoadAsset(UnityEngine.Object asset, System.Object param)
        {
            if (asset)
            {
                mainAsset = asset;
                LoadInfo info = (LoadInfo)param;
                loadedList.Add(info.name);
            }
            else
            {
                UnLoad();
            }
            SetOver();
        }

        private IEnumerator LoadBundle(RelationNode node, LayerLoadParam param)
        {
            if (node.Nodes.Count() > 0)
            {
                LayerLoadParam layerParam = new LayerLoadParam();
                layerParam.layer = param.layer + 1;
                layerParam.num = 0;

                for (int i = 0, count = node.Nodes.Count; i < count; i++)
                {
                    CoroutineHelper.CreateCoroutineHelper(LoadBundle(node.Nodes[i], layerParam));
                }

                while (layerParam.num >= 0 && layerParam.num < node.Nodes.Count)
                {
                    yield return null;
                }

                if (layerParam.num < 0)
                {
                    UnLoad();
                    param.num = -100000;
                    yield break;
                }
            }

            ResLoader.LoadByName(node.File, EndLoadBundle, new LoadInfo() { name = node.File, param = param }, true);
        }

        private void EndLoadBundle(UnityEngine.Object asset, System.Object param)
        {
            LoadInfo info = (LoadInfo)param;
            if (asset)
            {
                loadedList.Add(info.name);
                info.param.num++;
            }
            else
            {
                info.param.num = -1000000;
            }
        }

        internal void UnLoad()
        {
            if (mainAsset != null)
            {
                ResLoader.Unload(mainAsset);
                mainAsset = null;
            }

            for (int i = 0, count = loadedList.Count; i < count; i++)
            {
                ResLoader.RemoveAssetCacheByName(loadedList[i]);
            }

            loadedList.Clear();
        }
    }
}
