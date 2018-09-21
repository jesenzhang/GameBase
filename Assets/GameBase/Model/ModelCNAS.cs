
using System.Collections.Generic;
using UnityEngine;

namespace GameBase.Model
{
    public class ModelCNAS : ModelBase
    {
        private ModelCombine modelCombine = new ModelCombine();
        private ModelAnimation modelAnimation = new ModelAnimation();

        private SkinnedMeshRenderer[] smrArr = null;
        private bool show = false;



        protected override void Awake()
        {
            base.Awake();

            modelCombine.SetEndProcessModelCall(EndProcessModel, null);
        }

        private void EndProcessModel(Transform trans, object param)
        {
            show = true;
            if (trans)
            {
                base.ProcessModel(trans.gameObject);
                smrArr = trans.GetComponentsInChildren<SkinnedMeshRenderer>();
            }
            else
            {
                base.ProcessModel(null);
                smrArr = null;
            }
        }

        public void Wear(int root, string[] models, bool autoTemp)
        {
            GameObject go = LuaObjs.GetGameObject(root);
            if (go == null)
                return;

            Wear(go, models, autoTemp);
        }

        public void Wear(GameObject root, string[] models, bool autoTemp)
        {
            modelCombine.SetCombinedRoot(root);
            modelCombine.Wear(models, autoTemp);
        }

        public void SetAnimatorController(RuntimeAnimatorController controller)
        {
            if (modelAnimation == null)
                return;
            modelAnimation.SetAnimatorController(controller);
        }

        protected override void ProcessAnimationModule()
        {
            if (modelAnimation == null)
                return;
            modelAnimation.ProcessAnimationModule(body);
        }

        public void SetModel(int goID, RuntimeAnimatorController control)
        {
            GameObject go = LuaObjs.GetGameObject(goID);
            SetModel(go, control);
        }

        public void SetModel(GameObject go, RuntimeAnimatorController control)
        {
            if (go == null || control == null)
                return;

            if (modelAnimation != null)
                modelAnimation.SetAnimatorController(control);

            base.ProcessModel(go);
            EndProcessModel(go.transform, null);
        }

        public string GetAnimationName()
        {
            if (modelAnimation == null)
                return null;
            return modelAnimation.GetAnimationName();
        }

        public override float GetAnimationLength(string name)
        {
            if (modelAnimation == null)
                return 0;
            return modelAnimation.GetAnimationLength(name);
        }

        public void SetAnimationSpeed(float sp)
        {
            if (modelAnimation != null)
                modelAnimation.SetAnimationSpeed(sp);
        }

        public void PlayAnimation(string name, float fadeLength = 0.3F, int layer = -1, bool cross = true, bool force = false, float timeOffset = 0)
        {
            if (modelAnimation != null)
                modelAnimation.PlayAnimation(name, fadeLength, layer, cross, force, timeOffset);
        }

        public static void SetWearPartCount(int count)
        {
            ModelCombine.SetAssetPartCount(count);
        }

        public override void Show(bool v)
        {
            if(Config.Detail_Debug_Log())
                Debug.LogWarning("model cnas show 0->" + show + "^" + v);
            if (show == v)
                return;
            show = v;
            if(Config.Detail_Debug_Log())
                Debug.LogWarning("model cnas show 1->" + show + "^" + v);
            if (smrArr != null)
            {
                if(Config.Detail_Debug_Log())
                    Debug.LogWarning("model cnas show 2->" + show + "^" + v + "^" + smrArr.Length);
                SkinnedMeshRenderer smr;
                for (int i = 0; i < smrArr.Length; i++)
                {
                    smr = smrArr[i];
                    if (smr.enabled != show)
                        smr.enabled = show;
                }
            }
        }

        public void AddAnimation(string name, UnityEngine.Object asset)
        {
            if (Config.Detail_Debug_Log())
            {
                Debug.LogWarning("model cnas add animation 0->" + name + "^" + (asset == null) + "^" + (modelAnimation == null));
                if (asset != null)
                    Debug.LogWarning("model cnas add animation type->" + asset);
            }

            if (modelAnimation != null)
            {
                //if (asset is Animation)
                //Animation anim = asset as Animation;
                AnimationClip anim = asset as AnimationClip;
                if(anim != null)
                {
                    if (Config.Detail_Debug_Log())
                        Debug.LogWarning("model cnas add animation 1->" + name);
                    modelAnimation.AddAnimation(name, anim);
                }
            }
        }

        public void SetUpdateOffScreen(bool v)
        {
            if (smrArr != null)
            {
                for (int i = 0; i < smrArr.Length; i++)
                {
                    smrArr[i].updateWhenOffscreen = v;
                }
            }
        }

        public void AddAnimation(string name, Animation anim)
        {
            if (modelAnimation != null)
                modelAnimation.AddAnimation(name, anim.clip);
        }

        public void AddAnimation(string name, string resName)
        {
            if (modelAnimation != null)
                modelAnimation.AddAnimation(name, resName);
        }

        void Update()
        {
            if (modelAnimation != null)
                modelAnimation.Update();
        }

        void OnDestroy()
        {
            if (modelAnimation != null)
                modelAnimation.OnDestroy();
            if (modelCombine != null)
                modelCombine.OnDestroy();
            modelAnimation = null;
            modelCombine = null;
        }
    }
}
