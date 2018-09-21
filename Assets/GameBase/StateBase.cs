
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GameBase
{
    public abstract class StateBase
    {
        protected short state;

        public short CurState()
        {
            return state;
        }

        public abstract bool SetState(short s);

        public abstract void SetEvent(int ev, System.Object[] param);
    }
}