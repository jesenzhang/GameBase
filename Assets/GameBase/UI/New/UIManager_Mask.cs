using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameBase
{
    public static partial class UIManager
    {
        class LayerAlpha
        {
            public float alpha;
        }

        private static Dictionary<int, LayerAlpha> layerAlphaDic = new Dictionary<int, LayerAlpha>();

        private static int currentLayer = -1;
        private static float currentLowLayerAlpha = 1;
        
        public static void SetLessLayerUIAlpha(int layer, float alpha)
        {
            if (alpha < 0)
                return;

            if (layer < 0)
                layer = -layer;

            if (layer < currentLayer && alpha != currentLowLayerAlpha)
                return;

            currentLayer = layer;
            currentLowLayerAlpha = alpha;

            LayerAlpha la;
            if (!layerAlphaDic.TryGetValue(layer, out la))
            {
                la = new LayerAlpha();
                la.alpha = 1;
                layerAlphaDic.Add(layer, la);
            }

            Dictionary<int, List<UIFrame>>.Enumerator e = layerUI.GetEnumerator();
            UIFrame ui;
            int clayer;
            while (e.MoveNext())
            {
                clayer = e.Current.Key;
                if (clayer < 0)
                    clayer = -clayer;
                if (clayer < layer)
                {
                    if (!layerAlphaDic.TryGetValue(clayer, out la))
                    {
                        la = new LayerAlpha();
                        la.alpha = alpha;
                        layerAlphaDic.Add(clayer, la);
                    }

                    la.alpha = alpha;
                    for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                    {
                        ui = e.Current.Value[i];
                        if(ui.IsShowing())
                            ui.SetAlpha(alpha);
                    }
                }
            }
        }

        internal static void ResetUIAlpha(UIFrame ui)
        {
            int layer = ui.GetLayer();
            if (layer < 0)
                layer = -layer;

            currentLayer = -1;
            currentLowLayerAlpha = -1;

            Dictionary<int, List<UIFrame>>.Enumerator e = layerUI.GetEnumerator();
            int topLowLayer = -1;
            float lowAlpha = 1;
            UIFrame uii;
            float uiAlpha;
            bool doo = false;
            int clayer = -1;
            while (e.MoveNext())
            {
                doo = false;
                clayer = e.Current.Key;
                if (clayer < 0)
                    clayer = -clayer;
                if (clayer < layer)
                {
                    if (clayer > topLowLayer)
                    {
                        for (int i = 0, count = e.Current.Value.Count; i < count; i++)
                        {
                            uii = e.Current.Value[i];
                            if (uii.IsShowing())
                            {
                                uiAlpha = uii.GetLowLayerAlpha();
                                if (uiAlpha < lowAlpha)
                                    lowAlpha = uiAlpha;
                                if(!doo)
                                    doo = true;
                            }
                        }

                        if(doo)
                            topLowLayer = clayer;
                    }
                }
            }

            Dictionary<int, LayerAlpha>.Enumerator ex = layerAlphaDic.GetEnumerator();
            List<UIFrame> list;
            while (ex.MoveNext())
            {
                clayer = ex.Current.Key;
                if (clayer >= topLowLayer)
                {
                    if (ex.Current.Value.alpha != 1)
                    {
                        ex.Current.Value.alpha = 1;

                        if (layerUI.TryGetValue(clayer, out list))
                        {
                            for (int i = 0, count = list.Count; i < count; i++)
                            {
                                uii = list[i];
                                if (uii.IsShowing())
                                    uii.SetAlpha(1);
                            }
                        }

                        if (layerUI.TryGetValue(-clayer, out list))
                        {
                            for (int i = 0, count = list.Count; i < count; i++)
                            {
                                uii = list[i];
                                if (uii.IsShowing())
                                    uii.SetAlpha(1);
                            }
                        }
                    }
                }
            }

            if (topLowLayer >= 0)
            {
                SetLessLayerUIAlpha(topLowLayer, lowAlpha);
            }
        }

        internal static void ProcessUIAlpha(UIFrame ui)
        {
            int layer = ui.GetLayer();
            if (layer < 0)
                layer = -layer;
            LayerAlpha la;
            if (!layerAlphaDic.TryGetValue(layer, out la))
            {
                la = new LayerAlpha();
                la.alpha = 1;
                layerAlphaDic.Add(layer, la);
                return;
            }

            ui.SetAlpha(la.alpha);
        }
    }
}
