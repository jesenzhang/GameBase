#define LUASCRIPT
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public partial class UIFrame
    {
        private enum UIStatus
        {
            Creating = 1,
            Enabled = 2,
            Disabled = 3,
            Loading = 4,
        }

        private UIStatus status = UIStatus.Disabled;
        private string _name = "";
        public string name
        {
            get { return _name; }
        }

        private int uiID = -1;
        public int id
        {
            get { return uiID; }
        }

        private UIData uiData = null;
        private Transform m_transform = null;

        private List<short> atlases = new List<short>();
        private short atlasCount = 0;

        private List<short> textures = new List<short>();
        private short textureCount = 0;

        private short layer = 0;
        private short group = 0;

        private bool hasBG = false;
        private Color bgColor = new Color(0, 0, 0, 0);
        private bool bgCollider = false;
        private float lowLayerAlpha = 1;
        private float uiAlpha = 1;

        private bool listenOnDrag = false;
        private bool listenOnDragOver = false;
        private bool listenOnDragOut = false;
        private bool listenOnDragEnd = false;
        private bool listenOnDrop = false;
        private bool listenOnKey = false;
        private bool listenOnTooltip = false;
        private bool listenOnDragStart = false;
        private bool listenOnSroll = false;
        private bool listenOnSelect = false;
        private bool listenOnHover = false;
        private bool listenOnDoubleClick = false;
        private bool listenOnSubmit = false;
        private bool listenOnClick = false;
        private bool listenOnPress = false;


#if LUASCRIPT
        private LuaInterface.LuaFunction scriptShowUICall = null;
        private int scriptCallParam = -1;

        private LuaInterface.LuaFunction lua_OnCreate = null;
        private LuaInterface.LuaFunction lua_OnEnable = null;
        private LuaInterface.LuaFunction lua_OnShowOver = null;
        private LuaInterface.LuaFunction lua_OnDisable = null;
        private LuaInterface.LuaFunction lua_OnClick = null;
        private LuaInterface.LuaFunction lua_OnSubmit = null;
        private LuaInterface.LuaFunction lua_OnDoubleClick = null;
        private LuaInterface.LuaFunction lua_OnHover = null;
        private LuaInterface.LuaFunction lua_OnPress = null;
        private LuaInterface.LuaFunction lua_OnSelect = null;
        private LuaInterface.LuaFunction lua_OnScroll = null;
        private LuaInterface.LuaFunction lua_OnDragStart = null;
        private LuaInterface.LuaFunction lua_OnDrag = null;
        private LuaInterface.LuaFunction lua_OnDragOver = null;
        private LuaInterface.LuaFunction lua_OnDragOut = null;
        private LuaInterface.LuaFunction lua_OnDragEnd = null;
        private LuaInterface.LuaFunction lua_OnDrop = null;
        private LuaInterface.LuaFunction lua_OnKey = null;
        private LuaInterface.LuaFunction lua_OnTooltip = null;

#elif JSSCRIPT
        private JsRepresentClass jsRepresentClass = null
#endif

        private AtlasUnloadType atlasUnloadType = AtlasUnloadType.IMMEDIATELY;
        private float atlasUnloadInterval = 0;

        private bool dirty = false;



        internal UIFrame(int id, string name, List<string> atlas, List<string> texs)
        {
            _name = name;
            uiID = id;

            if (atlas != null)
            {
                short index = -1;
                for (int i = 0, count = atlas.Count; i < count; i++)
                {
                    index = GetAtlasIndex(atlas[i]);
                    if (index < 0)
                        Debugger.LogError("ui atlas is invalid->" + atlas[i]);
                    else
                        atlases.Add(index);
                }
            }

            if (texs != null)
            {
                short index = -1;
                for (int i = 0, count = texs.Count; i < count; i++)
                {
                    index = GetTextureIndex(texs[i]);
                    if (index < 0)
                        Debugger.LogError("ui texture is invalid->" + texs[i]);
                    else
                        textures.Add(index);
                }
            }


#if LUASCRIPT
            LuaManager.Require("UI/" + name);
#endif
        }

        internal void SetDirty()
        {
            dirty = true;
        }

        internal bool IsDirty()
        {
            return dirty;
        }

        internal void DirtyDel()
        {
            if (m_transform)
            {
                Object.Destroy(m_transform.gameObject);
            }
        }

        public int GetBaseDepth()
        {
            if (uiData == null)
                return -9999999;
            if(uiData.panels == null)
                return -9999999;

            if (uiData.panels.Count > 0)
            {
                int low = 9999999;
                UIPanel panel = null;
                for (int i = 0, count = uiData.panels.Count; i < count; i++)
                {
                    panel = uiData.panels[i];
                    if (panel.depth < low)
                        low = panel.depth;
                }

                return low;
            }
            else
                return -9999999;
        }

        public void SetBG(float r, float g, float b, float a, bool hasCollider)
        {
            if (hasBG)
                return;

            hasBG = true;
            bgCollider = hasCollider;
            bgColor = new Color(r, g, b, a);
        }

        public void SetLowLayerAlpha(float alpha)
        {
            lowLayerAlpha = alpha;
        }

        internal float GetLowLayerAlpha()
        {
            return lowLayerAlpha;
        }

        public void SetAtlasUnloadType(int t)
        {
            atlasUnloadType = (AtlasUnloadType)t;
        }

        public void SetAtlasUnloadDelay(float interval)
        {
            atlasUnloadInterval = interval;
        }

        public int GetPanelCount()
        {
            if (uiData != null)
            {
                if (uiData.panels != null)
                    return uiData.panels.Count;
            }

            return 0;
        }

        public int GetPanelDepth(int index)
        {
            if (uiData == null)
                return -1;

            if (uiData.panels == null)
                return -2;

            if (index < 0 || index >= uiData.panels.Count)
                return -3;

            UIPanel panel = uiData.panels[index];
            return panel.depth;
        }

        public void SetPanelDepthAndSortOrder(int index, int order)
        {
            if (uiData == null)
                return;

            if (uiData.panels == null)
                return;

            if (index < 0 || index >= uiData.panels.Count)
                return;

            UIPanel panel = uiData.panels[index];
            panel.depth = order;
            panel.sortingOrder = order;
        }

        public int GetUIStatus()
        {
            return (int)status;
        }

        public bool IsShowing()
        {
            return status == UIStatus.Enabled || status == UIStatus.Creating;
        }

        public Transform GetRoot()
        {
            return m_transform;
        }

        public Transform Find(string path)
        {
            if (uiData == null)
                return null;

            return m_transform.Find(path);
        }

        public Component FindComponent(string type, string path)
        {
            if (uiData == null)
                return null;

            Transform trans = m_transform.Find(path);
            if (trans == null)
                return null;

            return trans.GetComponent(type);
        }

        public Component FindComponent(string type, Transform trans)
        {
            if (trans == null)
                return null;

            return trans.GetComponent(type);
        }

        public Component FindComponent(string type, Transform gTrans, string path)
        {
            if (gTrans == null)
                return null;
            Transform trans = gTrans.Find(path);
            if (trans == null)
                return null;
            return trans.GetComponent(type);
        }

        public Transform DuplicateAndAdd(Transform go, Transform parent, int eventBegin)
        {
            if (uiData == null)
                return null;
            if (go == null)
                return null;

            if (parent == null)
                parent = m_transform;

            GameObject gg = (GameObject)Object.Instantiate(go.gameObject, parent, true);

            ProcessElement(gg, eventBegin);
            return gg.transform;
        }

        private void ProcessElement(GameObject go, int eventBegin)
        {
            UISpriteData[] spriteArr = go.GetComponentsInChildren<UISpriteData>();
            if (spriteArr != null && spriteArr.Length > 0)
                uiData.sprites.AddRange(spriteArr);

            UILabelData[] labelArr = go.GetComponentsInChildren<UILabelData>();
            if (labelArr != null && labelArr.Length > 0)
                uiData.labels.AddRange(labelArr);

            UIEvent[] evArr = go.GetComponentsInChildren<UIEvent>();
            if (evArr != null && evArr.Length > 0)
            {
                uiData.events.AddRange(evArr);
                UIEvent e = null;
                for (int i = 0, count = evArr.Length; i < count; i++)
                {
                    e = evArr[i];
                    e.id = eventBegin + i;
                    RegisterEvent(e);
                }
            }
        }

        public void SetLayer(short layer)
        {
            this.layer = layer;
        }

        internal int GetLayer()
        {
            return layer;
        }

        public void SetGroup(short group)
        {
            this.group = group;
        }

        internal int GetGroup()
        {
            return group;
        }

        internal void Show(bool show)
        {
            if (show)
            {
                if (status == UIStatus.Enabled)
                {
                    ShowUICallback();
                    return;
                }

                if (status == UIStatus.Loading)
                    return;

                LoadAsset();
            }
            else
            {
                if (status != UIStatus.Disabled)
                {
                    ProcessShow(show);

                    UIManager.ProcessUnShowUILayer(layer, this);

                    OnDisable();
                    status = UIStatus.Disabled;
                    ClearAtlas();
                    ClearTextures();

                    if (IsDirty())
                        UIManager.DeleteUI(this);
                }
            }
        }

        internal void SetScriptShowCall(LuaInterface.LuaFunction showCall, int scriptCallParam)
        {
#if LUASCRIPT
            scriptShowUICall = showCall;
            this.scriptCallParam = scriptCallParam;
#endif
        }

        private void OnEnable()
        {
            if (uiData == null)
                return;

            if (lowLayerAlpha < 1)
                UIManager.SetLessLayerUIAlpha(layer, lowLayerAlpha);

            UIManager.ProcessUIAlpha(this);

            UISpriteData data = null;
            for (int i = 0, count = uiData.sprites.Count; i < count; i++)
            {
                data = uiData.sprites[i];
                SetAtlas(data, true);
            }

            UITextureData utd = null;
            for (int i = 0, count = uiData.textures.Count; i < count; i++)
            {
                utd = uiData.textures[i];
                SetTexture(utd, true);
            }

#if JSSCRIPT
                jsRepresentClass.CallFunctionByFunName("OnEnable", this);
#elif LUASCRIPT
            if (lua_OnEnable == null)
                lua_OnEnable = LuaManager.GetFunction(name + ".OnEnable");
            if (lua_OnEnable != null)
                LuaManager.CallFunc_V(lua_OnEnable, this);
#endif
        }

        private void OnDisable()
        {
            if (uiData == null)
                return;
            UIManager.ResetUIAlpha(this);

            scriptShowUICall = null;
            UISpriteData data = null;
            for (int i = 0, count = uiData.sprites.Count; i < count; i++)
            {
                data = uiData.sprites[i];
                SetAtlas(data, false);
            }

#if JSSCRIPT
                jsRepresentClass.CallFunctionByFunName("OnDisable", this);
#elif LUASCRIPT
            if (lua_OnDisable == null)
                lua_OnDisable = LuaManager.GetFunction(name + ".OnDisable");
            if (lua_OnDisable != null)
                LuaManager.CallFunc_V(lua_OnDisable, this);
#endif
        }

        private void LoadAsset()
        {
            if (status != UIStatus.Disabled)
                return;
            status = UIStatus.Loading;
            atlasCount = 0;
            textureCount = 0;

            if (atlases.Count == 0 && textures.Count == 0)
            {
                LoadAssetOver();
                return;
            }
            for (int i = 0, count = atlases.Count; i < count; i++)
                RequestAtlas(atlases[i], RequestAtlasOver, atlasUnloadType, atlasUnloadInterval);

            for (int i = 0, count = textures.Count; i < count; i++)
                RequestTexture(textures[i], RequestTextureOver);
        }

        private void ShowUICallback()
        {
#if LUASCRIPT
            if (scriptShowUICall != null)
            {
                LuaInterface.LuaFunction func = scriptShowUICall;
                scriptShowUICall = null;
                LuaManager.CallFunc_VX(func, scriptCallParam);
                scriptCallParam = -1;
            }
#endif
        }

        private void LoadAssetOver()
        {
            if (uiData == null)
                Create();
            else
            {
                ProcessShow(true);
                Prepare();

                if (lua_OnShowOver == null)
                    lua_OnShowOver = LuaManager.GetFunction(name + ".OnShowOver");
                if (lua_OnShowOver != null)
                    LuaManager.CallFunc_V(lua_OnShowOver, this);
            }
        }

        private void Prepare()
        {
            status = UIStatus.Enabled;

            UIManager.ProcessUILayer(layer, this);

            OnEnable();

            ShowUICallback();
        }

        private void ClearAtlas()
        {
            for (int i = 0, count = atlases.Count; i < count; i++)
            {
                if (dirty)
                    RemoveAtlas(-atlases[i] - 1);
                else
                    RemoveAtlas(atlases[i]);
            }
        }

        private void ClearTextures()
        {
            for (int i = 0, count = textures.Count; i < count; i++)
            {
                if (dirty)
                    RemoveTexture(-textures[i] - 1);
                else
                    RemoveTexture(textures[i]);
            }
        }

        private void RequestAtlasOver()
        {
            atlasCount++;
            if (atlasCount == atlases.Count)
            {
                CheckAllOver();
            }
        }

        private void RequestTextureOver()
        {
            textureCount++;
            if (textureCount == textures.Count)
                CheckAllOver();
        }

        private void CheckAllOver()
        {
            if (atlasCount == atlases.Count)
            {
                if (textureCount == textures.Count)
                {
                    if (status != UIStatus.Loading)
                    {
                        atlasCount = 0;
                        textureCount = 0;
                        ClearAtlas();
                        ClearTextures();
                        return;
                    }

                    LoadAssetOver();
                    atlasCount = 0;
                    textureCount = 0;
                }
            }
        }

        private void ProcessShow(bool show)
        {
            if (uiData != null)
            {
                uiData.gameObject.SetActive(show);
            }
        }

        public void SetAlpha(float alpha)
        {
            if (uiAlpha == alpha)
                return;
            uiAlpha = alpha;
            UIPanel panel;
            for (int i = 0, count = uiData.panels.Count; i < count; i++)
            {
                panel = uiData.panels[i];
                panel.alpha = alpha;
            }
        }

        public void SetToggleBaseGroup(int baseGroup)
        {
            if (uiData == null)
                return;

            if (uiData.toggles == null)
                return;

            UIToggleData data;
            for (int i = 0, count = uiData.toggles.Count; i < count; i++)
            {
                data = uiData.toggles[i];
                if(data.originGroup != 0)
                    data.toggle.group = data.originGroup + baseGroup;
            }
        }

        private void Create()
        {
            status = UIStatus.Creating;
            ResLoader.LoadByName(name, (asset, param) =>
            {
                if (asset == null)
                    return;
                GameObject goo = ((GameObject)Object.Instantiate(asset));
                goo.name = name;
                uiData = goo.GetComponent<UIData>();
                if (uiData == null)
                {
                    Debugger.LogError("ui prefab has no UIData->" + name);
                    return;
                }

                for (int i = 0, count = uiData.events.Count; i < count; i++)
                    RegisterEvent(uiData.events[i]);

                for (int i = 0, count = uiData.labels.Count; i < count; i++)
                    uiData.labels[i].label.font = comFont;

                m_transform = uiData.transform;
                m_transform.parent = UIManager.GetUIRoot();

                GameCommon.ResetTrans(m_transform);
                UIPanel[] panels = m_transform.GetComponents<UIPanel>();
                if (panels.Length > 0)
                    uiData.panels.AddRange(panels);
                panels = m_transform.GetComponentsInChildren<UIPanel>();
                if (panels.Length > 0)
                    uiData.panels.AddRange(panels);

                if (layer >= 0)
                {
                    UIPanel panel = null;
                    for (int i = 0, count = uiData.panels.Count; i < count; i++)
                    {
                        panel = uiData.panels[i];
                        panel.sortingOrder = panel.depth;
                    }
                }

                UIManager.UICreateCall(id, this);


#if JSSCRIPT
                jsRepresentClass.CallFunctionByFunName("OnCreate", this);
#elif LUASCRIPT
                if (lua_OnCreate == null)
                    lua_OnCreate = LuaManager.GetFunction(name + ".OnCreate");
                if (lua_OnCreate != null)
                    LuaManager.CallFunc_V(lua_OnCreate, this);
#endif

                UIManager.RegisterUILayer(layer, this);

                if (hasBG)
                {
                    GameObject go = new GameObject();
                    go.layer = m_transform.gameObject.layer;
                    Transform strans = go.transform;
                    strans.parent = m_transform;
                    GameCommon.ResetTrans(strans);
                    strans.localScale = new Vector3(2000, 2000, 1);
                    UIColorSprite ucs = go.AddComponent<UIColorSprite>();
                    ucs.depth = -1;
                    ucs.SetShaderEnum(UIColorSprite.ShaderEnum.SH2, true);
                    ucs.SetColor(bgColor);

                    if (bgCollider)
                        go.AddComponent<BoxCollider>();
                }

                if (status != UIStatus.Creating)
                {
                    Debugger.Log("create show ui after disable");
                    ProcessShow(false);
                    ClearAtlas();
                    ClearTextures();
                    return;
                }

                Prepare();

                if (lua_OnShowOver == null)
                    lua_OnShowOver = LuaManager.GetFunction(name + ".OnShowOver");
                if (lua_OnShowOver != null)
                    LuaManager.CallFunc_V(lua_OnShowOver, this);
            }, null);
        }

        private void RegisterEvent(UIEvent e)
        {
            if (e != null)
            {
                e.onSubmit += OnSubmit;
                e.onClick += OnClick;
                e.onDoubleClick += OnDoubleClick;
                e.onHover += OnHover;
                e.onPress += OnPress;
                e.onSelect += OnSelect;
                e.onScroll += OnScroll;
                e.onDragStart += OnDragStart;
                e.onDrag += OnDrag;
                e.onDragOver += OnDragOver;
                e.onDragOut += OnDragOut;
                e.onDragEnd += OnDragEnd;
                e.onDrop += OnDrop;
                e.onKey += OnKey;
                e.onTooltip += OnTooltip;
            }
        }

        public void SetListenOnClick(bool flag)
        {
            listenOnClick = flag;
        }

        public void SetListenOnSubmit(bool flag)
        {
            listenOnSubmit = flag;
        }

        public void SetListenOnDoubleClick(bool flag)
        {
            listenOnDoubleClick = flag;
        }

        public void SetListenOnHover(bool flag)
        {
            listenOnHover = flag;
        }

        public void SetListenOnPress(bool flag)
        {
            listenOnPress = flag;
        }

        public void SetListenOnSelect(bool flag)
        {
            listenOnSelect = flag;
        }

        public void SetListenOnScroll(bool flag)
        {
            listenOnSroll = flag;
        }

        public void SetListenOnDragStart(bool flag)
        {
            listenOnDragStart = flag;
        }

        public void SetListenOnDrag(bool flag)
        {
            listenOnDrag = flag;
        }

        public void SetListenOnDragOver(bool flag)
        {
            listenOnDragOver = flag;
        }

        public void SetListenOnDragOut(bool flag)
        {
            listenOnDragOut = flag;
        }

        public void SetListenOnDragEnd(bool flag)
        {
            listenOnDragEnd = flag;
        }

        public void SetListenOnDrop(bool flag)
        {
            listenOnDrop = flag;
        }

        public void SetListenOnKey(bool flag)
        {
            listenOnKey = flag;
        }

        public void SetListenOnTooltip(bool flag)
        {
            listenOnTooltip = flag;
        }

        private void OnSubmit(GameObject gameObject, int id)
        {
            if (listenOnSubmit && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnSubmit", gameObject, id);
#elif LUASCRIPT
                if (lua_OnSubmit == null)
                    lua_OnSubmit = LuaManager.GetFunction(name + ".OnSubmit");
                if (lua_OnSubmit != null)
                    LuaManager.CallFunc_V(lua_OnSubmit, gameObject, id);
#endif
            }
        }

        private void OnClick(GameObject gameObject, int id)
        {
            if (listenOnClick && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnClick", gameObject, id);
#elif LUASCRIPT
                if (lua_OnClick == null)
                    lua_OnClick = LuaManager.GetFunction(name + ".OnClick");

                UIManager.Common_UIOnClick(uiID, id);

                if (lua_OnClick != null)
                    LuaManager.CallFunc_V(lua_OnClick, gameObject, id);
#endif
            }
        }

        private void OnDoubleClick(GameObject gameObject, int id)
        {
            if (listenOnDoubleClick && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDoubleClick", gameObject, id);
#elif LUASCRIPT
                if (lua_OnDoubleClick == null)
                    lua_OnDoubleClick = LuaManager.GetFunction(name + ".OnDoubleClick");
                if (lua_OnDoubleClick != null)
                    LuaManager.CallFunc_V(lua_OnDoubleClick, gameObject, id);
#endif
            }
        }

        private void OnHover(GameObject gameObject, bool isOver, int id)
        {
            if (listenOnHover && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnHover", gameObject, isOver, id);
#elif LUASCRIPT
                if (lua_OnHover == null)
                    lua_OnHover = LuaManager.GetFunction(name + ".OnHover");
                if (lua_OnHover != null)
                    LuaManager.CallFunc_V(lua_OnHover, gameObject, id);
#endif
            }
        }

        private void OnPress(GameObject gameObject, bool isPressed, int id)
        {
            if (listenOnPress && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnPress", gameObject, isPressed, id);
#elif LUASCRIPT
                if (lua_OnPress == null)
                    lua_OnPress = LuaManager.GetFunction(name + ".OnPress");
                if (lua_OnPress != null)
                    LuaManager.CallFunc_V(lua_OnPress, gameObject, isPressed, id);
#endif
            }
        }

        private void OnSelect(GameObject gameObject, bool selected, int id)
        {
            if (listenOnSelect && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnSelect", gameObject, selected, id);
#elif LUASCRIPT
                if (lua_OnSelect == null)
                    lua_OnSelect = LuaManager.GetFunction(name + ".OnSelect");
                if (lua_OnSelect != null)
                    LuaManager.CallFunc_V(lua_OnSelect, gameObject, selected, id);
#endif
            }
        }

        private void OnScroll(GameObject gameObject, float delta, int id)
        {
            if (listenOnSroll && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnScroll", gameObject, delta, id);
#elif LUASCRIPT
                if (lua_OnScroll == null)
                    lua_OnScroll = LuaManager.GetFunction(name + ".OnScroll");
                if (lua_OnScroll != null)
                    LuaManager.CallFunc_V(lua_OnScroll, gameObject, delta, id);
#endif
            }
        }

        private void OnDragStart(GameObject gameObject, int id)
        {
            if (listenOnDragStart && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDragStart", gameObject, id);

#elif LUASCRIPT
                if (lua_OnDragStart == null)
                    lua_OnDragStart = LuaManager.GetFunction(name + ".OnDragStart");
                if (lua_OnDragStart != null)
                    LuaManager.CallFunc_V(lua_OnDragStart, gameObject, id);
#endif
            }
        }

        private void OnDrag(GameObject gameObject, Vector2 delta, int id)
        {
            if (listenOnDrag && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDrag", gameObject, delta, id);
#elif LUASCRIPT
                if (lua_OnDrag == null)
                    lua_OnDrag = LuaManager.GetFunction(name + ".OnDrag");
                if (lua_OnDrag != null)
                    LuaManager.CallFunc_V(lua_OnDrag, gameObject, delta, id);
#endif
            }
        }

        private void OnDragOver(GameObject gameObject, int id)
        {
            if (listenOnDragOver && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDragOver", gameObject, id);
#elif LUASCRIPT
                if (lua_OnDragOver == null)
                    lua_OnDragOver = LuaManager.GetFunction(name + ".OnDragOver");
                if (lua_OnDragOver != null)
                    LuaManager.CallFunc_V(lua_OnDragOver, gameObject, id);
#endif
            }
        }

        private void OnDragOut(GameObject gameObject, int id)
        {
            if (listenOnDragOut && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDragOut", gameObject, id);
#elif LUASCRIPT
                if (lua_OnDragOut == null)
                    lua_OnDragOut = LuaManager.GetFunction(name + ".OnDragOut");
                if (lua_OnDragOut != null)
                    LuaManager.CallFunc_V(lua_OnDragOut, gameObject, id);
#endif
            }
        }

        private void OnDragEnd(GameObject gameObject, int id)
        {
            if (listenOnDragEnd && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDragEnd", gameObject, id);
#elif LUASCRIPT
                if (lua_OnDragEnd == null)
                    lua_OnDragEnd = LuaManager.GetFunction(name + ".OnDragEnd");
                if (lua_OnDragEnd != null)
                    LuaManager.CallFunc_V(lua_OnDragEnd, gameObject, id);
#endif
            }
        }

        private void OnDrop(GameObject gameObject, GameObject go, int id)
        {
            if (listenOnDrop && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnDrop", gameObject, go, id);
#elif LUASCRIPT
                if (lua_OnDrop == null)
                    lua_OnDrop = LuaManager.GetFunction(name + ".OnDrop");
                if (lua_OnDrop != null)
                    LuaManager.CallFunc_V(lua_OnDrop, gameObject, id);
#endif
            }
        }

        private void OnKey(GameObject gameObject, KeyCode key, int id)
        {
            if (listenOnKey && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnKey", gameObject, key, id);
#elif LUASCRIPT
                if (lua_OnKey == null)
                    lua_OnKey = LuaManager.GetFunction(name + ".OnKey");
                if (lua_OnKey != null)
                    LuaManager.CallFunc_V(lua_OnKey, gameObject, key, id);
#endif
            }
        }

        private void OnTooltip(GameObject gameObject, bool show, int id)
        {
            if (listenOnTooltip && !UIManager.IsFocusEventLocked(uiID, id))
            {
#if JSSCRIPT
                    jsRepresentClass.CallFunctionByFunName("OnTooltip", gameObject, show, id);
#elif LUASCRIPT
                if (lua_OnTooltip == null)
                    lua_OnTooltip = LuaManager.GetFunction(name + ".OnTooltip");
                if (lua_OnTooltip != null)
                    LuaManager.CallFunc_V(lua_OnTooltip, gameObject, show, id);
#endif
            }
        }
    }
}
