using System;
using System.Collections.Generic;

namespace GameBase
{
    public abstract class TimeBase
    {
        public abstract long GetServerTime();
        public abstract int GetDelay();
    }
}
