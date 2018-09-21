using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static partial class UIManager
    {
        private static Dictionary<int, List<UIFrame>> layerUI = new Dictionary<int, List<UIFrame>>();

        internal static void RegisterUILayer(int layer, UIFrame ui)
        {
            if (!layerUI.ContainsKey(layer))
                layerUI.Add(layer, new List<UIFrame>());

            List<UIFrame> list = layerUI[layer];
            list.Add(ui);
        }

        private static void UnRegisterUILayer(int layer, UIFrame ui)
        {
            if (ui == null)
                return;
            if (!layerUI.ContainsKey(layer))
                return;

            List<UIFrame> list = layerUI[layer];
            for (int i = 0, count = list.Count; i < count; i++)
            {
                if (list[i] == ui)
                {
                    list.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }

        internal static void ProcessUnShowUILayer(int layer, UIFrame ui)
        {
            if (layer < 0)
                return;
            Dictionary<int, List<UIFrame>>.Enumerator e = layerUI.GetEnumerator();
            UIFrame uf = null;
            int group = ui.GetGroup();
            List<UIFrame> dirtyUI = new List<UIFrame>();
            while (e.MoveNext())
            {
                if (e.Current.Key < 0)
                    continue;
                if (e.Current.Key > layer)
                {
                    for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                    {
                        uf = e.Current.Value[i];
                        uf.Show(false);
                        if (uf.IsDirty())
                            dirtyUI.Add(uf);
                    }
                }
            }
        }

        internal static void ProcessUILayer(int layer, UIFrame ui)
        {
            if (layer < 0)
                return;
            Dictionary<int, List<UIFrame>>.Enumerator e = layerUI.GetEnumerator();
            UIFrame uf = null;
            int group = ui.GetGroup();

            List<UIFrame> dirtyUI = new List<UIFrame>();
            while (e.MoveNext())
            {
                if (e.Current.Key < 0)
                    continue;
                if (e.Current.Key == layer)
                {
                    for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                    {
                        uf = e.Current.Value[i];
                        if (uf != ui)
                        {
                            if (group <= 0 || uf.GetGroup() != group)
                            {
                                uf.Show(false);
                                if (uf.IsDirty())
                                    dirtyUI.Add(uf);
                            }
                        }
                    }
                }
                else if (e.Current.Key > layer)
                {
                    for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                    {
                        uf = e.Current.Value[i];
                        if (group <= 0 || uf.GetGroup() != group)
                        {
                            uf.Show(false);
                            if (uf.IsDirty())
                                dirtyUI.Add(uf);
                        }
                    }
                }
            }

            for (int i = 0, count = dirtyUI.Count; i < count; i++)
            {
                UnRegisterUILayer(dirtyUI[i].GetLayer(), dirtyUI[i]);
            }
        }
    }
}
