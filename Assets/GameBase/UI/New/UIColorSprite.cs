using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    class UIColorSprite : UIWidget    {
        private Shader shader = null;

        public enum ShaderEnum
        {
            SH1 = 0,
            SH2 = 1,
            SH3 = 2,
        }

        private ShaderEnum shaderEnum = ShaderEnum.SH1;
        private string shaderName = "Transparent/Diffuse";

        public Material _material;


        public void SetShaderEnum(ShaderEnum em, bool set = false)
        {
            shaderEnum = em;
            ProcessShaderEnum(set);
        }

        private void ProcessShaderEnum(bool set = false)
        {
            switch (shaderEnum)
            {
                case ShaderEnum.SH1:
                    shaderName = "PF/DepthAlpha";
                    break;
                case ShaderEnum.SH2:
                    shaderName = "PF/Alpha";
                    break;
            }

            if (set && _material != null)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                    _material.shader = shader;
            }
        }

        private Color s_color = new Color(0, 0, 0, 0.5f);

        public void SetColor(Color s_color)
        {
            if (_material == null)
                OnStart();
            this.s_color = s_color;
            if (_material != null)
                _material.SetColor("_Color", s_color);
        }

        public void SetScale(Vector3 scale)
        {
            gameObject.transform.localScale = scale;
        }

        // override add
        override public int minWidth
        {
            get
            {
                return base.minWidth;
            }
        }

        override public int minHeight
        {
            get
            {
                return base.minHeight;
            }
        }
        // end override add

        protected override void OnStart()
        {
            if(Config.Detail_Debug_Log())
                Debugger.Log("----------------------------create ui color sprite material 0");
            if (_material == null)
            {
                ProcessShaderEnum();
                if(Config.Detail_Debug_Log())
                    Debugger.Log("----------------------------create ui color sprite material 1->" + shaderName + "^" + (shader == null));
                if (shader == null)
                    shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    if (Config.Detail_Debug_Log())
                        Debugger.Log("----------------------------create ui color sprite material 2");
                    _material = new Material(shader);
                    _material.SetColor("_Color", s_color);
                }
            }
        }

        public override Material material
        {
            get
            {
                if (_material == null)
                {
                    if (shader != null)
                    {
                        _material = new Material(shader);
                        _material.SetColor("_Color", s_color);
                    }
                }

                return _material;
            }
        }

        public override Texture mainTexture
        {
            get { return null; }
        }

        void OnDestroy()
        {
            Object.Destroy(_material);

            _material = null;
            RemoveFromPanel();
        }

        private void OnRender()
        {
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetColor(s_color);
        }

        public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
        {
            verts.Add(new Vector3(0.5f, 0.5f, 0));
            verts.Add(new Vector3(0.5f, -0.5f, 0));
            verts.Add(new Vector3(-0.5f, -0.5f, 0));
            verts.Add(new Vector3(-0.5f, 0.5f, 0));

            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);
            uvs.Add(Vector2.zero);

            cols.Add(Color.white);
            cols.Add(Color.white);
            cols.Add(Color.white);
            cols.Add(Color.white);
        }
    }
}
