using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameBase.Model
{
    internal class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        internal AnimationClipOverrides(int capacity) : base(capacity) { }

        internal AnimationClip this[string name]
        {
            get { return this.Find(x => x.Key.name.Equals(name)).Value; }
            set
            {
                int index = this.FindIndex(x => x.Key.name.Equals(name));
                if (index != -1)
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }


    internal class ModelAnimation
    {
        private string animationName;

        private Animator _anim = null;
        private Animator anim
        {
            get { return _anim; }
        }

        private short layer = 0;
        private float animBegin = 0;
        private float animLen = 0;
        private bool playing = false;

        private Dictionary<string, AnimationClip> animNameToClip = new Dictionary<string, AnimationClip>();
        private AnimatorOverrideController animatorController = null;


        internal void SetAnimatorController(RuntimeAnimatorController controller)
        {
            if (animatorController != null)
                return;
            animatorController = new AnimatorOverrideController(controller);

            if (animatorController.animationClips != null)
            {
                AnimationClip ac = null;
                for (int i = 0, count = animatorController.animationClips.Length; i < count; i++)
                {
                    ac = animatorController.animationClips[i];
                    if (animNameToClip.ContainsKey(ac.name))
                        continue;
                    animNameToClip.Add(ac.name, ac);
                }
            }
        }

        private void ResetAnimation()
        {
            _anim.Play("Take 001", -1, 0);
            _anim.Update(0);
        }

        private void EndLoadAnimation(UnityEngine.Object asset, object param)
        {
            if (animatorController == null)
                return;
            if (asset == null)
                return;
            string name = (string)param;
            if (animNameToClip.ContainsKey(name))
                return;
            Animation anim = (Animation)asset;
            animNameToClip.Add(name, anim.clip);

            ApplyAnimation();
        }

        private void ApplyAnimation()
        {
            AnimationClipOverrides clips = new AnimationClipOverrides(animatorController.clips.Length);
            animatorController.GetOverrides(clips);
            Dictionary<string, AnimationClip>.Enumerator e = animNameToClip.GetEnumerator();
            while (e.MoveNext())
            {
                clips[e.Current.Key] = e.Current.Value;
            }

            if (Config.Detail_Debug_Log())
                Debug.LogWarning("begin model animation apply animation->" + animatorController.clips.Length + "^" + animNameToClip.Count + "^" + clips.Count);
            animatorController.ApplyOverrides(clips);
            if (Config.Detail_Debug_Log())
                Debug.LogWarning("end model animation apply animation->" + animatorController.clips.Length + "^" + animNameToClip.Count + "^" + clips.Count);
        }

        internal void AddAnimation(string name, AnimationClip anim)
        {
            if (Config.Detail_Debug_Log())
                Debug.LogWarning("model animation add animation 0->" + name + "^" + (anim == null));
            if (anim == null)
                return;
            if (animNameToClip.ContainsKey(name))
                return;
            if (Config.Detail_Debug_Log())
                Debug.LogWarning("model animation add animation 1");
            animNameToClip.Add(name, anim);

            ApplyAnimation();
        }

        internal void AddAnimation(string name, string resName)
        {
            if (animNameToClip.ContainsKey(name))
                return;

            ResLoader.LoadByName(resName, EndLoadAnimation, name);
        }

        internal void RemoveAnimation(string name)
        {
            if (!animNameToClip.ContainsKey(name))
                return;

            animNameToClip.Remove(name);

            ApplyAnimation();
        }

        internal void ProcessAnimationModule(Transform body)
        {
            _anim = body.GetComponent<Animator>();
            if (_anim == null)
                _anim = body.GetComponentInChildren<Animator>();

            if (_anim)
                _anim.runtimeAnimatorController = animatorController;
        }

        internal string GetAnimationName()
        {
            return animationName;
        }

        internal float GetAnimationLength(string name)
        {
            AnimationClip ac;
            if (!animNameToClip.TryGetValue(name, out ac))
                return 0;

            return ac.length;
        }

        internal void SetAnimationSpeed(float sp)
        {
            if (_anim == null)
                return;

            _anim.speed = sp;
        }

        public void PlayAnimation(string name, float fadeLength = 0.3F, int layer = -1, bool cross = true, bool force = false, float timeOffset = 0)
        {
            if (_anim == null)
                return;

            if (!force && (name == animationName && playing))
                return;

            animLen = GetAnimationLength(name);
            if (animLen <= 0)
                return;
            animBegin = Time.time;
            animationName = name;

            if (layer < 0)
            {
                layer = _anim.GetLayerIndex("Base Layer");
            }
            AnimatorStateInfo info = _anim.GetCurrentAnimatorStateInfo(layer);
            float progress = info.normalizedTime - ((int)info.normalizedTime);
            if (progress < 0.2f)
                cross = false;

            if (!cross)
                _anim.Play(name, layer, timeOffset);
            else
                _anim.CrossFade(name, fadeLength, layer, timeOffset);
        }

        internal void Update()
        {
            if (playing)
            {
                float time = Time.time;
                if (time - animBegin >= animLen)
                {
                    playing = false;
                }
            }
        }

        internal void OnDestroy()
        {
        }
    }
}
