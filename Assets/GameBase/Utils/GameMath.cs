
using UnityEngine;
using System.Collections.Generic;

namespace GameBase
{
    public static class GameMath
    {
        public static void VectorRotate90(float x, float y, bool plus, out float ox, out float oy)
        {
            if (plus)
            {
                ox = -y;
                oy = x;
            }
            else
            {
                ox = y;
                oy = -x;
            }
        }

        public static void VectorRotate(float x, float y, float angle, out float ox, out float oy)
        {
            float radian = (float)(angle * (Mathf.PI / 180));
            float cos = (float)Mathf.Cos(radian);
            float sin = (float)Mathf.Sin(radian);

            ox = x * cos - y * sin;
            oy = x * sin + y * cos;
        }

        public static void VectorRotate(float x, float y, float sin, float cos, out float ox, out float oy)
        {
            ox = x * cos - y * sin;
            oy = x * sin + y * cos;
        }
    }
}
