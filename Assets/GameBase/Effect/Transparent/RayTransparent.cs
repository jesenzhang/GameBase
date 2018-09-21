
using UnityEngine;
using System.Collections.Generic;

namespace GameBase
{
    public class RayTransparent
    {
        private string shaderName = "Custom/TransparentDiffuse";
        private Shader shader = null;
        private float transparent = 0.5f;

        class Info
        {
            private Material material;
            private Shader shader;

            public Material GetMaterial()
            {
                return material;
            }

            public Shader GetShader()
            {
                return shader;
            }

            public Info(Material mat, Shader sh)
            {
                material = mat;
                shader = sh;
            }
        }

        class Infos
        {
            public List<Info> list = new List<Info>();
            public float processTime = 0;
            public bool alpha = false;
            public Renderer renderer = null;
        }

        private Dictionary<Collider, Infos> processed;

        public RayTransparent()
        {
            shader = ShaderManager.Find(shaderName);
            processed = new Dictionary<Collider, Infos>();
        }

        public void SetShader(string name)
        {
            shaderName = name;
            shader = ShaderManager.Find(name);
        }

        public void SetTransparent(float v)
        {
            transparent = v;
        }

        public void Ray(Vector3 begin, Vector3 end, int rayLayerMask, int alphaLayerMask)
        {
            RaycastHit[] hits;
            float dis = Vector3.Distance(begin, end);
            Vector3 dir = (end - begin).normalized;

            hits = Physics.RaycastAll(begin, dir, dis, rayLayerMask);

            float curTime = Time.realtimeSinceStartup;

            if (hits != null && hits.Length > 0)
            {
                RaycastHit hit;
                Collider col;
                for (int i = 0, count = hits.Length; i < count; i++)
                {
                    hit = hits[i];
                    col = hit.collider;
                    if (processed.ContainsKey(col))
                    {
                        processed[col].processTime = curTime;
                    }
                    else
                    {
                        if ((alphaLayerMask & (1 << col.gameObject.layer)) != 0)
                        {
                            Renderer renderer = col.GetComponentInChildren<Renderer>();
                            if (renderer)
                            {
                                Infos infos = new Infos();
                                Material mat;
                                for (int j = 0, jcount = renderer.materials.Length; j < jcount; j++)
                                {
                                    mat = renderer.materials[j];
                                    Info info = new Info(mat, mat.shader);
                                    infos.list.Add(info);

                                    mat.shader = shader;
                                    mat.SetFloat("_Alpha", transparent);
                                }

                                infos.processTime = curTime;
                                infos.alpha = true;
                                processed.Add(col, infos);
                            }
                        }
                        else
                        {
                            Infos infos = new Infos();
                            Renderer renderer = col.GetComponentInChildren<Renderer>();
                            if (renderer)
                            {
                                infos.renderer = renderer;
                                infos.renderer.enabled = false;

                                infos.processTime = curTime;
                                infos.alpha = false;

                                processed.Add(col, infos);
                            }
                        }
                    }
                }
            }

            List<Collider> del = new List<Collider>();
            Dictionary<Collider, Infos>.Enumerator e = processed.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Value.processTime != curTime)
                {
                    del.Add(e.Current.Key);

                    if (e.Current.Value.alpha)
                    {
                        Info info;
                        for (int i = 0, count = e.Current.Value.list.Count; i < count; i++)
                        {
                            info = e.Current.Value.list[i];
                            info.GetMaterial().shader = info.GetShader();
                        }
                    }
                    else
                    {
                        if (e.Current.Value.renderer)
                            e.Current.Value.renderer.enabled = true;
                    }
                }
            }

            for (int i = 0, count = del.Count; i < count; i++)
            {
                processed.Remove(del[i]);
            }
        }
    }
}
