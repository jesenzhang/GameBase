
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Example;

namespace GameBase
{
    public static class GameCommon
    {
        private static Vector3 v3_zero = Vector3.zero;
        private static Vector3 v3_one = Vector3.one;
        private static Quaternion q_identity = Quaternion.identity;

        private static int random_seed = 0;

        private static float mapOriginX = 0;
        private static float mapOriginZ = 0;


        private static int mainThreadID = -1;



        public static void Init()
        {
            mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsMainThread(int tid)
        {
            return ((mainThreadID >= 0) && (tid == mainThreadID));
        }

        public static bool IsMainThread()
        {
            int tid = GameUtils.GetCurrentThreadID();
            return IsMainThread(tid);
        }

        public static System.Random random
        {
            get
            {
                if (random_seed == 0)
                {
                    random_seed = (int)GameTime.time_F;
                    if (random_seed == 0)
                        random_seed = (int)Time.realtimeSinceStartup;
                }
                System.Random rand = new System.Random(random_seed);
                random_seed += rand.Next(1, 53);
                return rand;
            }
        }


        public static void SetMapOriginPos(float x, float z)
        {
            mapOriginX = x;
            mapOriginZ = z;
        }

        public static void ResetTrans(Transform trans)
        {
            if (trans == null)
                return;

            trans.localPosition = v3_zero;
            trans.localRotation = q_identity;
            trans.localScale = v3_one;
        }


        private static int Extra(float v)
        {
            return (int)(v - (int)v + 0.5f);
        }

        public static int Coord_X(float v)
        {
            v -= mapOriginX;
            return (int)v * 2 + Extra(v);
        }

        public static int Coord_Y(float v)
        {
            v -= mapOriginZ;
            return (int)v * 2 + Extra(v);
        }

        private static float RealByLogic(int v)
        {
            return v / 2 + 0.25f + 0.5f * (v % 2);
        }

        private static float RealByLogic_X(int v) 
        {
            return RealByLogic(v) + mapOriginX;
        }

        private static float RealByLogic_Z(int v) 
        {
            return RealByLogic(v) + mapOriginZ;
        }

        public static Vector3 PositionByLogicCoord(int x, int y) 
        {
            Vector3 vec = new Vector3();
            vec.x = RealByLogic_X(x);
            vec.y = 0;
            vec.z = RealByLogic_Z(y);

            return vec;
        }

        /*
        public static Vector2i LogicCoordByPosition(Vector3 position)
        {
            return new Vector2i() { x = Coord_X(position.x), y = Coord_Y(position.z) };
        }
        */

        public delegate void CB_ErgodicTrans(Transform trans, ref int arg1, System.Object param);

        public static void ErgodicTransform(Transform parent, CB_ErgodicTrans cb, int total, ref int cur, System.Object param)
        {
            if (parent == null)
                return;
            if (cb == null)
                return;

            if (parent.childCount > 0)
            {
                foreach (Transform t in parent)
                {
                    cb(t, ref cur, param);
                    if (cur >= total)
                        return;

                    ErgodicTransform(t, cb, total, ref cur, param);
                }
            }
        }

        public static short SwapInt16(this short n)
        {
            return (short)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        public static ushort SwapUInt16(this ushort n)
        {
            return (ushort)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        public static int SwapInt32(this int n)
        {
            return (int)(((SwapInt16((short)n) & 0xffff) << 0x10) |
                          (SwapInt16((short)(n >> 0x10)) & 0xffff));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MapItem
    {
        public enum Type
        {
            PLAYER = 1,
            NPC = 2,
        }

        public int id;
        public int selfFaction;
        public short curx;
        public short cury;
        public short index;
        public byte type;
        public byte dead;
    }
}
