
using System.Collections.Generic;

namespace GameBase
{
    public class TweenValue : UITweener
    {
        public delegate void TweenCall(float factor, bool isFinished);
        private TweenCall tweenCall = null;

        public void SetDelegateFunc(TweenCall call)
        {
            tweenCall = call;
        }

        public void AddDelegateFunc(TweenCall call)
        {
            tweenCall += call;
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            if (tweenCall != null)
                tweenCall(factor, isFinished);
        }
    }
}
