
using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public static partial class UIManager
    {
        private const string UIATLASDATAFILE = "UIAtlasData.zl";
        private const string UILABELTEXTFILE = "UITextData.txt";
        private const string UIATLASENAMES = "AtlasNames";
        private const string UITEXTURENAMES = "TextureNames";

        private static Dictionary<string, List<string>> uiInfo = null;
        private static List<string> labelTextData = new List<string>();

        private static Dictionary<string, UIFrame> allUI = new Dictionary<string, UIFrame>();
        private static Dictionary<int, UIFrame> indexToUI = new Dictionary<int, UIFrame>();

        private static List<int> ignoreCheckDepthUI = new List<int>();

        private static Transform uiRootTrans = null;
        private static Camera _camera = null;
        private static UICamera _uiCamera = null;

        private static UIFont comFont = null;

        private static int focusEventLockNum = 0;
        class FocusEvent
        {
            private bool _set = false;
            public bool set
            {
                get { return _set; }
                set
                {
                    if (_set == value)
                        return;
                    _set = value;
                    if (_set)
                        AddFocus();
                    else
                        DeleteFocus();
                }
            }

            private void AddFocus()
            {
                focusEventLockNum++;
            }

            private void DeleteFocus()
            {
                focusEventLockNum--;
                if (focusEventLockNum < 0)
                    focusEventLockNum = 0;
            }
        }
        private static Dictionary<int, Dictionary<int, FocusEvent>> focusEvent = new Dictionary<int, Dictionary<int, FocusEvent>>();

        private static LuaInterface.LuaFunction initUICall = null;
        private static LuaInterface.LuaFunction createUICall = null;
        private static LuaInterface.LuaFunction uiOnClickCall = null;

        private static bool _lockEvent = false;

        public static void ClearData()
        {
            Debugger.Log("clear ui data");
            uiInfo.Clear();
            labelTextData.Clear();

            //for loading ui etc. which are show before update unshow first
            UIFrame.CacheOldUIData();

            //set old ui dirty to delete
            Dictionary<string, UIFrame>.Enumerator e = allUI.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.SetDirty();
            }
        }

        public static void ClearOldCacheData()
        {
            UIFrame.ClearOldCacheData();
        }

        public static void AddIgnoreCheckUIDepth(int index)
        {
            ignoreCheckDepthUI.Add(index);
        }

        public static void RemoveIgnoreCheckUIDepth(int index)
        {
            ignoreCheckDepthUI.Remove(index);
        }

        public static bool IsUIToppest(int index)
        {
            UIFrame ui;
            if (indexToUI.TryGetValue(index, out ui))
                return false;

            int depth = ui.GetBaseDepth();

            Dictionary<int, UIFrame>.Enumerator e = indexToUI.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Key == index)
                    continue;
                
                if (!e.Current.Value.IsShowing())
                    continue;

                if (ignoreCheckDepthUI.Contains(e.Current.Key))
                    continue;

                if (depth < e.Current.Value.GetBaseDepth())
                    return false;
            }

            return true;
        }

        public static UIFont GetComFont()
        {
            return comFont;
        }

        public static void SetInitUIFunc(LuaInterface.LuaFunction initUIFunc)
        {
            initUICall = initUIFunc;
        }

        public static void SetUICreateFunc(LuaInterface.LuaFunction createUIFunc)
        {
            createUICall = createUIFunc;
        }

        public static void SetUIOnClickFunc(LuaInterface.LuaFunction onClickFunc)
        {
            uiOnClickCall = onClickFunc;
        }

        internal static void Common_UIOnClick(int uiID, int btnID)
        {
            if (uiOnClickCall != null)
                LuaManager.CallFunc_V(uiOnClickCall, uiID, btnID);
        }

        public static void Init()
        {
            bool atlasDataInited = false;
            bool labelDataInited = false;
            bool fontInited = false;

            if (!uiRootTrans)
            {
                GameObject go = GameObject.Find("UI Root");
                if (go == null)
                {
                    Debugger.LogError("UI Root is null");
                    return;
                }

                Object.DontDestroyOnLoad(go);

                uiRootTrans = go.transform;

                Transform camTrans = uiRootTrans.Find("Camera");
                if (camTrans == null)
                {
                    Debugger.LogError("Cam trans is null");
                    return;
                }

                _camera = camTrans.GetComponent<Camera>();
                _uiCamera = camTrans.GetComponent<UICamera>();
            }
            else
                fontInited = true;

            System.Action checkOver = () =>
            {
                if (atlasDataInited && labelDataInited && fontInited)
                {
                    List<string> atlases;
                    if (!uiInfo.TryGetValue(UIATLASENAMES, out atlases))
                    {
                        Debugger.LogError(UIATLASDATAFILE + " has no field->" + UIATLASENAMES);
                        return;
                    }

                    List<string> textures;
                    if (!uiInfo.TryGetValue(UITEXTURENAMES, out textures))
                    {
                        Debugger.LogError(UIATLASDATAFILE + " has no field->" + UITEXTURENAMES);
                        return;
                    }

                    UIFrame.Init(atlases, textures);
                    uiInfo.Remove(UIATLASENAMES);
                    uiInfo.Remove(UITEXTURENAMES);

                    Debugger.Log("UI Init Over");
                    MessagePool.CSSendMessage(null, MessagePool.UIInitOver, Message.FilterTypeNothing, null);
                }
            };

            if (!fontInited)
            {
                ResLoader.LoadByName("MSYH", (asset, param) =>
                {
                    if (asset == null)
                    {
                        Debugger.Log("init font failed");
                        return;
                    }

                    comFont = ((GameObject)asset).GetComponent<UIFont>();
                    if (comFont == null)
                    {
                        Debugger.Log("font data not contains UIFont");
                        return;
                    }

                    UIFrame.SetFont(comFont);

                    fontInited = true;

                    checkOver();
                }, null);
            }

            ResLoader.AsynReadBytesByName(UIATLASDATAFILE, (arr, param) =>
            {
                if (arr == null)
                {
                    Debugger.LogError("init UI atlas data failed");
                    return;
                }

                ZLText zlt = new ZLText(arr);
                uiInfo = zlt.ReadAll();
                zlt.Dispose();

                atlasDataInited = true;

                checkOver();
            }, null, true);

            ResLoader.AsynReadUTF8ByName(UILABELTEXTFILE, (str, param) =>
            {
                if (str == null)
                {
                    Debugger.LogError("init UI label text data failed");
                    return;
                }

                string[] lines = null;
                int index = str.IndexOf("\r\n");
                if (index >= 0)
                    lines = str.Split(new string[] { "\r\n" }, System.StringSplitOptions.None);
                else
                    lines = str.Split(new string[] { "\n" }, System.StringSplitOptions.None);

                labelTextData.AddRange(lines);
                labelDataInited = true;

                checkOver();
            }, null, true);
        }

        public static void AddUnloadAtlas(string name)
        {
            UIFrame.AddUnloadAtlas(name);
        }

        public static void SetEventLock(bool v)
        {
            _lockEvent = v;
        }

        public static void SetFocusEventLock(int uiID, int btnID)
        {
            int k1 = uiID + btnID;
            int k2 = uiID - btnID;

            Dictionary<int, FocusEvent> dic;
            if (!focusEvent.TryGetValue(k1, out dic))
            {
                dic = new Dictionary<int, FocusEvent>();
                focusEvent.Add(k1, dic);
            }

            FocusEvent fe;
            if (dic.TryGetValue(k2, out fe))
            {
                fe.set = true;
            }
            else
            {
                fe = new FocusEvent();
                fe.set = true;
                dic.Add(k2, fe);
            }

            if (focusEventLockNum == 1)
            {
            }
        }

        public static void ReleaseFocusEventLock(int uiID, int btnID)
        {
            int k1 = uiID + btnID;
            int k2 = uiID - btnID;

            Dictionary<int, FocusEvent> dic;
            if (!focusEvent.TryGetValue(k1, out dic))
                return;

            FocusEvent fe;
            if (!dic.TryGetValue(k2, out fe))
                return;

            fe.set = false;

            if (focusEventLockNum == 0)
            {
            }
        }

        public static void ReleaseAllFocusEventLock()
        {
            Dictionary<int, Dictionary<int, FocusEvent>>.Enumerator e = focusEvent.GetEnumerator();
            while (e.MoveNext())
            {
                Dictionary<int, FocusEvent>.Enumerator e1 = e.Current.Value.GetEnumerator();
                while (e1.MoveNext())
                {
                    e1.Current.Value.set = false;
                }
            }

            focusEventLockNum = 0;

            if (focusEventLockNum == 0)
            {
            }
        }

        public static bool IsFocusEventLocked(int uiID, int btnID)
        {
            if(Config.Debug_Log())
                GameBase.Debugger.LogWarning("is focus event lock->" + _lockEvent + "^" + uiID + "^" + btnID + "^" + focusEventLockNum);
            if (focusEventLockNum > 0)
            {
                int k1 = uiID + btnID;
                int k2 = uiID - btnID;

                Dictionary<int, FocusEvent> dic;
                if (focusEvent.TryGetValue(k1, out dic))
                {
                    FocusEvent fe;
                    if (dic.TryGetValue(k2, out fe))
                    {
                        return !fe.set;
                    }
                }

                return true;
            }
            else
                return _lockEvent;
        }

        public static bool IsEventLocked()
        {
            return _lockEvent;
        }

        public static void ShowUI(string name, int index)
        {
            UIFrame ui = GetUI(name, index);
            ProcessUIShow(ui, true);
        }

        public static void ShowUI(string name, int index, LuaInterface.LuaFunction showUICall, int scriptCallParam)
        {
            UIFrame ui = GetUI(name, index);
            ui.SetScriptShowCall(showUICall, scriptCallParam);
            ProcessUIShow(ui, true);
        }

        public static void UnShowUI(string name, int index)
        {
            UIFrame ui = GetUI(name, index);
            ProcessUIShow(ui, false);
        }

        public static void UnShowUI(UIFrame ui)
        {
            ProcessUIShow(ui, false);
        }

        public static void UnShowAll()
        {
            Dictionary<int, UIFrame>.Enumerator e = indexToUI.GetEnumerator();
            while (e.MoveNext())
            {
                ProcessUIShow(e.Current.Value, false);
            }
        }

        public static void UnShowAllExceptThese(int[] indexes)
        {
            if (indexes == null || indexes.Length == 0)
            {
                UnShowAll();
            }
            else
            {
                Dictionary<int, UIFrame>.Enumerator e = indexToUI.GetEnumerator();
                while (e.MoveNext())
                {
                    bool find = false;
                    for (int i = 0, count = indexes.Length; i < count; i++)
                    {
                        if (indexes[i] == e.Current.Key)
                        {
                            find = true;
                            break;
                        }
                    }

                    if(!find)
                        ProcessUIShow(e.Current.Value, false);
                }
            }
        }

        public static int GetUIStatus(string name, int index)
        {
            UIFrame ui = GetUI(name, index);
            return ui.GetUIStatus();
        }

        public static void UnShowLayerUI(int layer)
        {
            ProcessUILayer(layer, null);
        }

        internal static Transform GetUIRoot()
        {
            return uiRootTrans;
        }

        public static Camera GetCamera()
        {
            return _camera;
        }

        public static UICamera GetUICamera()
        {
            return _uiCamera;
        }

        public static int GetCurrentTouchID()
        {
            return UICamera.currentTouchID;
        }

        private static UIFrame GetUI(string name, int index)
        {
            UIFrame ui = null;
            if (!indexToUI.ContainsKey(index))
            {
                List<string> atlases;
                uiInfo.TryGetValue(name, out atlases);

                List<string> textures;
                string texName = name + "_IMG";
                uiInfo.TryGetValue(texName, out textures);

                if (atlases != null && textures != null)
                {
                    ui = new UIFrame(index, name, atlases, textures);
                    uiInfo.Remove(name);
                    uiInfo.Remove(texName);
                    allUI.Add(name, ui);
                    indexToUI.Add(index, ui);
                    if (initUICall != null)
                        LuaManager.CallFunc_VX(initUICall, index, ui);
                }
            }
            else
            {
                ui = allUI[name];
            }

            return ui;
        }

        public static void DeleteUI(UIFrame ui)
        {
            if (ui == null)
                return;
            if (ui.IsShowing())
                ui.Show(false);

            ui.DirtyDel();
            allUI.Remove(ui.name);
            indexToUI.Remove(ui.id);
        }

        private static void ProcessUIShow(UIFrame ui, bool show)
        {
            if (ui == null)
                return;

            ui.Show(show);
        }

        internal static void UICreateCall(int index, UIFrame ui)
        {
            if (createUICall != null)
                LuaManager.CallFunc_VX(createUICall, index, ui);
        }
    }
}
