using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaInterface
{
    public partial class LuaState
    {
        public IntPtr GetL()
        {
            return L;
        }
    }
}
