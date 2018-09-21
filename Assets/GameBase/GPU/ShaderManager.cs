using System.Collections.Generic;
using UnityEngine;

namespace GameBase
{
    public static class ShaderManager
    {
        private static bool initing = false;
        private static short curIndex = 0;
        private static string[] shaderGroup = null;
        private static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

        private static AssetBundle curAssetBundle = null;
        private static string curAssetBundleName = null;
        private static int curShadersCount = 0;
        private static int curShadersLoaded = 0;

        public static void Init(string[] _shaderGroup)
        {
            if (initing)
                return;
            if (_shaderGroup == null)
                return;
            if (_shaderGroup.Length == 0)
                return;

            initing = true;
            curIndex = 0;
            shaderGroup = _shaderGroup;
            LoadShaders();
        }

        public static Shader Find(string name)
        {
            if (name == null)
                return null;
            Shader shader;
            if (shaders.TryGetValue(name, out shader))
            {
                return shader;
            }

            return Shader.Find(name);
        }

        private static void LoadShaders()
        {
            if(curAssetBundle != null)
            {
                curAssetBundle = null;
                curShadersCount = 0;
                curShadersLoaded = 0;
            }

            if (curIndex >= shaderGroup.Length)
            {
                initing = false;
                Shader.WarmupAllShaders();
                return;
            }

            curAssetBundleName = shaderGroup[curIndex];
            
            curIndex++;
            ResLoader.LoadByName(curAssetBundleName, EndLoadShaders, null, true);
        }

        private static void EndLoadShaders(Object asset, object param)
        {
            if (asset != null)
            {
                curAssetBundle = (AssetBundle)asset;
                ResLoader.HelpLoadAsset(curAssetBundle, "Assets/Shaders.asset", EndLoadShaderNameAsset, null, typeof(ShaderContentHolder));
            }
        }

        private static void EndLoadShaderNameAsset(AssetBundleRequest asset, object param)
        {
            if (asset == null || asset.asset == null)
            {
                Debugger.LogError("shader asset is invalid->" + (string)param);
                return;
            }

            ShaderContentHolder holder = (ShaderContentHolder)asset.asset;
            curShadersCount = holder.assetPaths.Length;
            for (int i = 0; i < curShadersCount; i++)
            {
                ResLoader.HelpLoadAsset(curAssetBundle, holder.assetPaths[i], EndLoadShaderAsset, holder.shaderNames[i], typeof(Shader));
            }
        }

        private static void PlusLoadedShaderNum()
        {
            curShadersLoaded++;
            if (curShadersLoaded >= curShadersCount)
            {
                LoadShaders();
            }
        }

        private static void EndLoadShaderAsset(AssetBundleRequest asset, object param)
        {
            if (asset == null || asset.asset == null)
            {
                PlusLoadedShaderNum();
                return;
            }

            string name = (string)param;
            if (shaders.ContainsKey(name))
            {
                PlusLoadedShaderNum();
                return;
            }

            Shader shader = (Shader)asset.asset;
            shaders.Add(name, shader);
            PlusLoadedShaderNum();
        }
    }
}
