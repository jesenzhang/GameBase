using System;
using System.Collections.Generic;

namespace GameBase
{
    public class SSector
    {
        class Vec2
        {
            internal float x;
            internal float y;

            internal Vec2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            internal void Normalize()
            {
                float magnitude = x * x + y * y;
                if (magnitude > 0)
                {
                    magnitude = (float)(1 / Math.Sqrt(magnitude));
                    x *= magnitude;
                    y *= magnitude;
                }
            }

            internal float SquareDistance(Vec2 v)
            {
                float p1 = x - v.x;
                float p2 = y - v.y;

                return p1 * p1 + p2 * p2;
            }

            internal float SquareDistance(float x, float y)
            {
                float p1 = this.x - x;
                float p2 = this.y - y;

                return p1 * p1 + p2 * p2;
            }
        }

        private Vec2 origin;
        private Vec2 p1;
        private Vec2 p2;
        private Vec2 pDir;
        private Boolean lessPI;
        private float squareRadius;

        public SSector(float x, float y, float angle, float dirX, float dirY, float r)
        {
            if (angle <= 180)
                lessPI = true;
            else
                lessPI = false;
            float radian = (float)(angle * (Math.PI / 180));
            float cos = (float)Math.Cos(radian);
            float sin = (float)Math.Sin(radian);
            squareRadius = r * r;
            pDir = new Vec2(x + dirX, y + dirY);

            origin = new Vec2(x, y);

            p1 = new Vec2(dirX * cos - dirY * sin + x, dirX * sin + dirY * cos + y);
            p2 = new Vec2(dirX * cos + dirY * sin + x, dirY * cos - dirX * sin + y);
        }

        private float PointVectorSideV(Vec2 v1, Vec2 v2, Vec2 v)
        {
            return (v1.x - v.x) * (v2.y - v.y) - (v1.y - v.y) * (v2.x - v.x);
        }

        private Boolean PointInSector(Vec2 v1, Vec2 v2, Vec2 origin, Vec2 v)
        {
            float re = PointVectorSideV(origin, v1, v);
            if (re >= 0)
                return false;
            re = PointVectorSideV(origin, v2, v);
            if (re <= 0)
                return false;

            return true;
        }

        public Boolean PointInSector(float x, float y)
        {
            float dis = origin.SquareDistance(x, y);
            if (dis > squareRadius)
                return false;

            if (lessPI)
                return PointInSector(p1, p2, origin, new Vec2(x, y));
            else
            {
                Vec2 v = new Vec2(x, y);
                if (!PointInSector(p1, pDir, origin, v))
                {
                    if (!PointInSector(pDir, p2, origin, v))
                        return false;
                    else
                        return true;
                }
                else
                    return true;
            }
        }
    }
}
