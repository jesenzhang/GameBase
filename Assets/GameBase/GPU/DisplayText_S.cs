

using UnityEngine;

using System.Collections.Generic;

using GameBase;
using Example;


namespace GameBase
{
    public class DisplayText_S
    {
        private static float m_fYOffset = 2.0f;
        private static float m_fZOffset = 0f;

        private static TexturePacker packInfo = null;
        private static Dictionary<int, Example.PackItem> pack_data = new Dictionary<int, Example.PackItem>();

        private static List<Example.PackItem> process_list = new List<Example.PackItem>();

        private static Example.PackItem[] items = new Example.PackItem[20];

        private static Vector2 display_size = new Vector2(0, 0);

        private static int standardItem = -1;

        private static int camObjID = -1;


        private static void LoadComparison(string fileName, string prefix)
        {
            ResLoader.AsynReadBytesByName(fileName, EndLoadComparison, prefix);
        }

        private static void EndLoadComparison(byte[] data, object param)
        {
            if (data == null)
                return;

            string prefix = (string)param;

            ZLText zlt = new ZLText(data);
            Dictionary<string, List<string>> zlt_data = zlt.ReadAll();
            zlt = null;

            LoadData(prefix + ".bytes", zlt_data);
        }

        private static void LoadData(string fileName, object param)
        {
            ResLoader.AsynReadBytesByName(fileName, EndLoadData, param);
        }

        private static void EndLoadData(byte[] data, object param)
        {
            if (data == null)
                return;

            Dictionary<string, List<string>> comparison = (Dictionary<string, List<string>>)param;

            System.IO.MemoryStream ms = new System.IO.MemoryStream(data);
            packInfo = TexturePacker.Deserialize(ms);

            Dictionary<string, int> dic = new Dictionary<string, int>();
            for (int i = 0, count = packInfo.Names.Count; i < count; i++)
            {
                dic.Add(packInfo.Names[i], i);
            }

            packInfo.Names.Clear();

            Dictionary<string, List<string>>.Enumerator e = comparison.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value != null && e.Current.Value.Count > 0)
                {
                    int key = int.Parse(e.Current.Key);
                    int v;
                    if (dic.TryGetValue(e.Current.Value[0], out v))
                    {
                        pack_data.Add(key, packInfo.Items[v]);
                    }
                }
            }

            packInfo.Items.Clear();
            packInfo = null;

            Example.PackItem item;
            if (pack_data.TryGetValue(standardItem, out item))
            {
                GPUBillboardBuffer_S.SetStandardWH(item.Width, item.Height);
            }
            else
                Debug.LogError("gpu display text standard item id is invalid->" + standardItem);
        }

        private static void LoadTexture(string texFileName)
        {
            ResLoader.LoadByName(texFileName, EndLoadTex, null);
        }

        private static void EndLoadTex(UnityEngine.Object asset, object param)
        {
            if (asset == null)
                return;
            Texture tex = (Texture)asset;

            GPUBillboardBuffer_S.Instance().SetTexture(tex);
        }

        public static void Init(string fileName, uint maxSize, int _standardItem)
        {
            standardItem = _standardItem;
            GPUBillboardBuffer_S.InitInstance(maxSize);
            LoadComparison(fileName + "_comparison.zl", fileName);
            LoadTexture(fileName);
        }

        public static void SetConfig(float[] configs)
        {
            GPUBillboardBuffer_S gbs = GPUBillboardBuffer_S.Instance();
            if (gbs)
                gbs.SetConfigs(configs);
        }

        public static void SetLayer(string layerName)
        {
            GPUBillboardBuffer_S gbs = GPUBillboardBuffer_S.Instance();
            if (gbs)
                gbs.SetLayer(layerName);
        }

        public static void SetCamera(int id)
        {
            camObjID = id;
        }

        public static int Display(int luaObjID, int[] icids, float scale, Vector3 targetPos, float offsetX, bool hasScale)
        {
            if (icids == null)
                return -1;
            int index = 0;
            Example.PackItem item;
            for (int i = 0, count = icids.Length; i < count; i++)
            {
                if (pack_data.TryGetValue(icids[i], out item))
                {
                    items[index] = item;
                    index++;
                }
            }
            Vector3 initPos = new Vector3(targetPos.x, targetPos.y + m_fYOffset, targetPos.z + m_fZOffset);

            display_size.x = scale;
            display_size.y = scale;

            GPUBillboardBuffer_S.Instance().DisplayNumber(luaObjID, camObjID, items, index, display_size, initPos, Color.white, hasScale, offsetX);

            return 0;
        }
    }
}


