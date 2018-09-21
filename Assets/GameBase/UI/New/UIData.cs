using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public class UIData : MonoBehaviour
    {
        public List<UILabelData> labels = new List<UILabelData>();
        public List<UISpriteData> sprites = new List<UISpriteData>();
        public List<UIEvent> events = new List<UIEvent>();
        public List<UIPanel> panels = new List<UIPanel>();
        public List<UITextureData> textures = new List<UITextureData>();
        public List<UIToggleData> toggles = new List<UIToggleData>();
    }
}
