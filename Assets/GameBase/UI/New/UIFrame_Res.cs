using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public partial class UIFrame
    {
        enum AtlasUnloadType
        {
            IMMEDIATELY = 0,
            TIME = 1,
            SCENELOAD = 2,
            NOTUNLOAD = 3,
        }

        class AtlasData
        {
            internal UIAtlas atlas;
            internal short referenceNum;
            internal short index;
            internal string atlasName;
            internal AtlasUnloadType unloadType;
            internal float intervalTime;
            internal float beginTime;
        }

        class UnloadData
        {
            internal short index;
            internal short count;
            internal AtlasData data;
        }

        class TextureData
        {
            internal Texture texture;
            internal short referenceNum;
            internal short index;
            internal string textureName;
        }

        class UnloadTexData
        {
            internal short index;
            internal short count;
            internal TextureData data;
        }

        //atlas
        private static Dictionary<string, short> atlasNameToIndex = new Dictionary<string, short>();
        private static Dictionary<short, string> atlasIndexToName = new Dictionary<short, string>();
        private static List<AtlasData> atlasList = new List<AtlasData>();
        private static Dictionary<int, List<System.Action>> loadCallDic = new Dictionary<int, List<System.Action>>();
        private static List<UnloadData> unloadList = new List<UnloadData>();

        //texture
        private static Dictionary<string, short> textureNameToIndex = new Dictionary<string, short>();
        private static Dictionary<short, string> textureIndexToName = new Dictionary<short, string>();
        private static List<TextureData> textureList = new List<TextureData>();
        private static Dictionary<int, List<System.Action>> loadTextureCallDic = new Dictionary<int, List<System.Action>>();
        private static List<UnloadTexData> unloadTexList = new List<UnloadTexData>();


        private static List<AtlasData> tempAtlasList = null;
        private static List<TextureData> tempTextureList = null;

        private static List<string> oldIndexAtlasName = null;
        private static List<string> oldIndexTextureName = null;


        private static UIFont comFont = null;

        private static bool inited = false;


        internal static void CacheOldUIData()
        {
            AtlasData ad = null;
            
            oldIndexAtlasName = new List<string>();
            for (int i = 0, count = atlasList.Count; i < count; i++)
            {
                ad = atlasList[i];
                if (ad != null)
                {
                    oldIndexAtlasName.Add(ad.atlasName);
                }
            }

            TextureData td = null;
            oldIndexTextureName = new List<string>();
            for (int i = 0, count = textureList.Count; i < count; i++)
            {
                td = textureList[i];
                if (td != null)
                {
                    oldIndexTextureName.Add(td.textureName);
                }
            }
        }

        internal static void ClearOldCacheData()
        {
            if (oldIndexAtlasName != null)
            {
                oldIndexAtlasName.Clear();
                oldIndexAtlasName = null;
            }
            if (oldIndexTextureName != null)
            {
                oldIndexTextureName.Clear();
                oldIndexTextureName = null;
            }
            if (tempAtlasList != null)
            {
                tempAtlasList.Clear();
                tempAtlasList = null;
            }
            if (tempTextureList != null)
            {
                tempTextureList.Clear();
                tempTextureList = null;
            }
        }

        private static void BeforeInit()
        {
            //prev atlas
            AtlasData ad = null;
            tempAtlasList = new List<AtlasData>();
            for (int i = 0, count = atlasList.Count; i < count; i++)
            {
                ad = atlasList[i];
                if (ad != null && ad.referenceNum > 0)
                    tempAtlasList.Add(ad);
            }

            UnloadData uld;
            for (int i = 0, count = unloadList.Count; i < count; i++)
            {
                uld = unloadList[i];
                if (uld != null && uld.count > 0)
                {
                    ResLoader.RemoveAssetCacheByName(uld.data.atlasName);
                }
            }

            tempTextureList = new List<TextureData>();
            TextureData td = null;
            //prev texture
            for (int i = 0, count = textureList.Count; i < count; i++)
            {
                td = textureList[i];
                if (td != null && td.referenceNum > 0)
                    tempTextureList.Add(td);
            }

            UnloadTexData ultd = null;
            for (int i = 0, count = unloadTexList.Count; i < count; i++)
            {
                ultd = unloadTexList[i];
                if (ultd != null && ultd.count > 0)
                {
                    ResLoader.RemoveAssetCacheByName(ultd.data.textureName, false, false);
                }
            }
        }

        private static AtlasData FindAtlasData(string name)
        {
            AtlasData ad = null;
            for (int i = 0, count = atlasList.Count; i < count; i++)
            {
                ad = atlasList[i];
                if (ad != null && ad.atlasName == name)
                    return ad;
            }

            return null;
        }

        private static TextureData FindTextureData(string name)
        {
            TextureData td = null;
            for (int i = 0, count = textureList.Count; i < count; i++)
            {
                td = textureList[i];
                if (td != null && td.textureName == name)
                    return td;
            }

            return null;
        }

        private static void AfterInit()
        {
            AtlasData ad = null, tad = null;
            for (int i = 0, count = tempAtlasList.Count; i < count; i++)
            {
                ad = atlasList[i];
                if (ad != null)
                {
                    tad = FindAtlasData(ad.atlasName);
                    if (tad != null)
                    {
                        tad.referenceNum += ad.referenceNum;
                        tempAtlasList.RemoveAt(i);
                        i--;
                    }
                }
            }

            TextureData td = null, ttd = null;
            for (int i = 0, count = tempTextureList.Count; i < count; i++)
            {
                td = textureList[i];
                if (td != null)
                {
                    ttd = FindTextureData(td.textureName);
                    if (ttd != null)
                    {
                        ttd.referenceNum += td.referenceNum;
                        tempTextureList.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        internal static void Init(List<string> list, List<string> textures)
        {
            if (!inited)
                BeforeInit();

            //atlas
            Dictionary<string, int> temp = new Dictionary<string, int>();
            AtlasData data = null;
            for (int i = 0, count = atlasList.Count; i < count; i++)
            {
                data = atlasList[i];
                if (data.referenceNum > 0)
                {
                    temp.Add(data.atlasName, data.referenceNum);
                }
            }

            atlasIndexToName.Clear();
            atlasNameToIndex.Clear();
            atlasList.Clear();
            unloadList.Clear();

            string str;
            for (int i = 0, count = list.Count; i < count; i++)
            {
                str = list[i];
                atlasIndexToName.Add((short)i, str);
                atlasNameToIndex.Add(str, (short)i);
                data = new AtlasData() { index = (short)i, referenceNum = 0, atlas = null, atlasName = str, unloadType = AtlasUnloadType.IMMEDIATELY };
                if (temp.Count > 0)
                {
                    int tv;
                    if (temp.TryGetValue(str, out tv))
                    {
                        data.referenceNum = (short)tv;
                    }
                }
                atlasList.Add(data);
                unloadList.Add(new UnloadData() { index = (short)i, count = -1, data = data });
            }

            //texture
            temp.Clear();
            TextureData td = null;
            for (int i = 0, count = textureList.Count; i < count; i++)
            {
                td = textureList[i];
                if (td.referenceNum > 0)
                {
                    temp.Add(td.textureName, td.referenceNum);
                }
            }

            textureIndexToName.Clear();
            textureNameToIndex.Clear();
            textureList.Clear();
            unloadTexList.Clear();

            for (int i = 0, count = textures.Count; i < count; i++)
            {
                str = textures[i];
                textureIndexToName.Add((short)i, str);
                textureNameToIndex.Add(str, (short)i);
                td = new TextureData() { index = (short)i, referenceNum = 0, texture = null, textureName = str };
                if (temp.Count > 0)
                {
                    int tv;
                    if (temp.TryGetValue(str, out tv))
                    {
                        data.referenceNum = (short)tv;
                    }
                }
                textureList.Add(td);
                unloadTexList.Add(new UnloadTexData() { index = (short)i, count = -1, data = td });
            }

            if (!inited)
                AfterInit();

            if (!inited)
            {
                Timer.CreateTimer(1, -1, DoUpdate, null);
                inited = true;
            }
        }

        internal static void SetFont(UIFont font)
        {
            comFont = font;
        }

        private static short GetAtlasIndex(string name)
        {
            short index = -1;
            if (!atlasNameToIndex.TryGetValue(name, out index))
                return -1;

            return index;
        }

        private static short GetTextureIndex(string name)
        {
            short index = -1;
            if (!textureNameToIndex.TryGetValue(name, out index))
                return -1;

            return index;
        }

        private static void SetAtlas(UISpriteData data, bool set)
        {
            if (set)
            {
                AtlasData atlasData = atlasList[data.index];
                data.sprite.atlas = atlasData.atlas;
                data.sprite.Update();
            }
            else
                data.sprite.atlas = null;
        }

        private static void SetTexture(UITextureData data, bool set)
        {
            if (set)
            {
                TextureData textureData = textureList[data.id];
                data.texture.mainTexture = textureData.texture;
            }
            else
                data.texture.mainTexture = null;
        }

        private static void ProcessAtlasUnloadType(AtlasData atlasData, AtlasUnloadType unloadType, float param_f)
        {
            if (atlasData == null)
                return;

            int curu = (int)(atlasData.unloadType);
            int newu = (int)unloadType;
            if (curu >= newu)
                return;

            atlasData.unloadType = unloadType;
            if (unloadType == AtlasUnloadType.TIME)
            {
                atlasData.beginTime = Time.realtimeSinceStartup;
                if (param_f > atlasData.intervalTime)
                    atlasData.intervalTime = param_f;
            }
        }

        #region texture
        private static void RequestTexture(short index, System.Action callback)
        {
            if (index < 0 || index >= textureList.Count)
            {
                Debugger.LogError("request texture is invalid->" + index);
                return;
            }


            TextureData data = textureList[index];
            if (data.texture != null)
            {
                if (data.referenceNum < 0)
                    data.referenceNum = 0;
                data.referenceNum++;
                if (callback != null)
                    callback();
                return;
            }

            if (index < 0)
                return;

            string name;
            if (!textureIndexToName.TryGetValue(index, out name))
                return;
            string destName;
            string path;
            int size;
            bool encrypt;
            Example.VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null || path == "")
            {
                Debugger.LogError("ui texture is null->" + name);
                return;
            }

            if (!loadTextureCallDic.ContainsKey(index))
                loadTextureCallDic.Add(index, new List<System.Action>());

            List<System.Action> list = loadTextureCallDic[index];
            list.Add(callback);
            UnloadTexData ulData = unloadTexList[index];
            if (ulData.count >= 0)
                ulData.count = -1;
            if (list.Count == 1)
            {
                ResLoader.LoadByPath(name, destName, path, fileType, size, (asset, param) =>
                {
                    if (param == null)
                        return;

                    Texture tex = (Texture)asset;
                    ProcessTexture(tex, index);
                }, index);
            }
        }

        private static void ProcessTexture(Texture texture, int index)
        {
            if (texture != null)
            {
                if (index < 0 || index >= textureList.Count)
                {
                    Debugger.LogError("ui texture index is invalid->" + index);
                    return;
                }

                TextureData data = textureList[index];
                if (data.texture == null)
                    data.texture = texture;
                else
                {
                    Debug.LogError("mulitiple load texture->" + texture.name);
                    return;
                }

                if (loadTextureCallDic.ContainsKey(index))
                {
                    List<System.Action> list = loadTextureCallDic[index];
                    if (list.Count == 0)
                    {
                        Debugger.LogError("load ui texture call zero->" + texture.name + "^" + index);
                        RemoveTexture(index);
                        return;
                    }

                    if (data.referenceNum < 0)
                        data.referenceNum = 0;
                    data.referenceNum += (short)list.Count;
                    System.Action callback = null;
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        callback = list[i];
                        if (callback != null)
                            callback();
                    }

                    list.Clear();
                }
                else
                {
                    RemoveTexture(index);
                    Debugger.LogError("load ui texture call not contains->" + texture.name);
                }
            }
            else
            {
            }
        }

        private static void RemoveTexture(int index)
        {
            bool dirtyRemove = false;
            if (index < 0)
            {
                dirtyRemove = true;
                index = -index - 1;
            }

            if (dirtyRemove)
            {
                if (oldIndexTextureName == null)
                    return;
                if (index < 0 || index >= oldIndexTextureName.Count)
                {
                    Debugger.LogError("remove ui texture error->" + index);
                    return;
                }

                string name = oldIndexTextureName[index];
                ResLoader.RemoveAssetCacheByName(name, false, false);
            }
            else
            {
                if (index < 0 || index >= textureList.Count)
                {
                    Debugger.LogError("remove ui texture error->" + index);
                    return;
                }

                TextureData data = textureList[index];
                data.referenceNum--;
                Debug.LogError("remove atlas->" + data.textureName + "^" + data.referenceNum);

                if (data.referenceNum <= 0)
                {
                    UnloadTexData unloadData = unloadTexList[index];
                    unloadData.index = (short)index;
                    unloadData.count = 1;
                }
            }
        }

        private static void _DeleteTexture(TextureData data)
        {
            if (data.texture != null)
            {
                data.texture = null;
            }

            string name;
            if (!textureIndexToName.TryGetValue(data.index, out name))
            {
                Debugger.LogError("delete ui texture index to name not find->" + data.index);
                return;
            }

            ResLoader.RemoveAssetCacheByName(name, true, true);
        }
        #endregion

        #region atlas
        private static void RequestAtlas(short index, System.Action callback, AtlasUnloadType unloadType, float param_f)
        {
            if (index < 0 || index >= atlasList.Count)
            {
                Debugger.LogError("request atlas is invalid->" + index);
                return;
            }

            AtlasData data = atlasList[index];
            if (data.atlas != null)
            {
                if (data.referenceNum < 0)
                    data.referenceNum = 0;
                data.referenceNum++;
                ProcessAtlasUnloadType(data, unloadType, param_f);
                if (callback != null)
                    callback();
                return;
            }
            if (index < 0)
                return;

            if (!atlasIndexToName.ContainsKey(index))
                return;

            string name = atlasIndexToName[index];

            string destName;
            string path;
            int size;
            bool encrypt;
            Example.VersionFile.Type fileType;
            ResUpdate.GetLoadDetails(name, out destName, out path, out size, out encrypt, out fileType);
            if (path == null || path == "")
            {
                Debugger.LogError("ui atlas is null->" + name);
                return;
            }

            if (!loadCallDic.ContainsKey(index))
                loadCallDic.Add(index, new List<System.Action>());

            List<System.Action> list = loadCallDic[index];
            list.Add(callback);
            UnloadData ulData = unloadList[index];
            if (ulData.count >= 0)
                ulData.count = -1;
            if (list.Count == 1)
            {
                ResLoader.LoadByPath(name, destName, path, fileType, size, (asset, param) =>
                {
                    if (param == null)
                        return;

                    GameObject go = (GameObject)asset;
                    Object.DontDestroyOnLoad(go);
                    UIAtlas a = go.GetComponent<UIAtlas>();
                    if (a == null)
                        Debugger.LogError("ui atlas is invalid->" + name);
                    ProcessAtlas(a, index, unloadType, param_f);
                }, index);
            }
        }

        private static void ProcessAtlas(UIAtlas atlas, int index, AtlasUnloadType unloadType, float param_f)
        {
            if (atlas != null)
            {
                if (index < 0 || index >= atlasList.Count)
                {
                    Debugger.LogError("ui atlas index is invalid->" + index);
                    return;
                }

                AtlasData data = atlasList[index];
                if (data.atlas == null)
                    data.atlas = atlas;
                else
                {
                    Debug.LogError("mulitiple load atlas->" + atlas.name);
                    return;
                }

                ProcessAtlasUnloadType(data, unloadType, param_f);

                if (loadCallDic.ContainsKey(index))
                {
                    List<System.Action> list = loadCallDic[index];
                    if (list.Count == 0)
                    {
                        Debugger.LogError("load ui atlas call zero->" + atlas.name + "^" + index);
                        RemoveAtlas(index);
                        return;
                    }

                    if (data.referenceNum < 0)
                        data.referenceNum = 0;
                    data.referenceNum += (short)list.Count;
                    System.Action callback = null;
                    for (int i = 0, count = list.Count; i < count; i++)
                    {
                        callback = list[i];
                        if (callback != null)
                            callback();
                    }

                    list.Clear();
                }
                else
                {
                    RemoveAtlas(index);
                    Debugger.LogError("load ui atlas call not contains->" + atlas.name);
                }
            }
            else
            {
            }
        }

        private static void RemoveAtlas(int index)
        {
            bool dirtyRemove = false;
            if (index < 0)
            {
                dirtyRemove = true;
                index = -index - 1;
            }

            if (dirtyRemove)
            {
                if (oldIndexAtlasName == null)
                    return;
                if (index < 0 || index >= oldIndexAtlasName.Count)
                {
                    Debugger.LogError("dirty remove ui atlas error->" + index);
                    return;
                }

                string atlasName = oldIndexAtlasName[index];
                ResLoader.RemoveAssetCacheByName(atlasName, false, false);
            }
            else
            {
                if (index < 0 || index >= atlasList.Count)
                {
                    Debugger.LogError("remove ui atlas error->" + index);
                    return;
                }

                AtlasData data = atlasList[index];
                data.referenceNum--;

                bool doo = true;
                switch (data.unloadType)
                {
                    case AtlasUnloadType.NOTUNLOAD:
                    case AtlasUnloadType.SCENELOAD:
                        {
                            doo = false;
                        }
                        break;
                    case AtlasUnloadType.TIME:
                    case AtlasUnloadType.IMMEDIATELY:
                        {
                        }
                        break;
                }

                if (doo && data.referenceNum <= 0)
                {
                    UnloadData unloadData = unloadList[index];
                    unloadData.index = (short)index;
                    unloadData.count = 1;
                }
            }
        }

        private static void _DeleteAtlas(AtlasData data)
        {
            if (data.atlas != null)
            {
                data.atlas = null;
            }

            if (!atlasIndexToName.ContainsKey(data.index))
            {
                Debugger.LogError("delete ui atlas index to name not find->" + data.index);
                return;
            }

            string name = atlasIndexToName[data.index];
            ResLoader.RemoveAssetCacheByName(name, true, true);
        }
        #endregion

        internal static void AddUnloadAtlas(string name)
        {
            short index = -1;
            if (atlasNameToIndex.ContainsKey(name))
                index = atlasNameToIndex[name];

            if (index < 0)
                return;

            RequestAtlas(index, null, AtlasUnloadType.NOTUNLOAD, 0);
        }

        internal static void SceneLoadProcessAtlases()
        {
            AtlasData data = null;
            for (int i = 0, count = atlasList.Count; i < count; i++)
            {
                data = atlasList[i];
                if (data.unloadType == AtlasUnloadType.SCENELOAD)
                {
                    data.referenceNum = 0;
                    _DeleteAtlas(data);
                }
            }
        }

        private static void DoUpdate(object param)
        {
            UnloadData data = null;
            float curTime = Time.realtimeSinceStartup;
            for (int i = 0, count = unloadList.Count; i < count; i++)
            {
                data = unloadList[i];
                if (data == null)
                    continue;
                if (data.count < 0)
                    continue;

                if (data.count == 0)
                {
                    switch (data.data.unloadType)
                    {
                        case AtlasUnloadType.TIME:
                            {
                                if ((curTime - data.data.beginTime) < data.data.intervalTime)
                                    continue;
                            }
                            break;
                    }

                    if (data.data.referenceNum <= 0)
                    {
                        _DeleteAtlas(data.data);
                    }
                }

                data.count--;
            }

            UnloadTexData ultd;
            for (int i = 0, count = unloadTexList.Count; i < count; i++)
            {
                ultd = unloadTexList[i];
                if (ultd == null)
                    continue;
                if (ultd.count < 0)
                    continue;

                if (ultd.count == 0)
                {
                    if (ultd.data.referenceNum <= 0)
                    {
                        _DeleteTexture(ultd.data);
                    }
                }

                ultd.count--;
            }
        }
    }
}
