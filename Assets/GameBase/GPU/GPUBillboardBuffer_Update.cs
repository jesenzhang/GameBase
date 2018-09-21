
using UnityEngine;
using System.Collections.Generic;

namespace GameBase
{
    public partial class GPUBillboardBuffer_S
    {
        class TempUpdate
        {
            internal int objID;
            internal int camObjID;
            internal uint begin;
            internal uint end;
            internal float beginTime;
            internal Vector3 localCenter;
        }

        private List<TempUpdate> updateList;

        private Matrix4x4 localToWorldMat = new Matrix4x4();


        private TempUpdate GenTempUpdate()
        {
            TempUpdate tu = null;
            for (int i = 0, count = updateList.Count; i < count; i++)
            {
                tu = updateList[i];
                if (tu == null)
                {
                    tu = new TempUpdate();
                    tu.objID = -1;
                    updateList[i] = tu;
                    break;
                }
                else
                {
                    if (tu.objID < 0)
                    {
                        break;
                    }
                }
            }

            if (tu == null)
            {
                tu = new TempUpdate();
                tu.objID = -1;
                updateList.Add(tu);
            }

            return tu;
        }

        private bool CalculateWorldPos(int luaObjID, int camObjID, Vector3 localPos, out Vector3 worldPos)
        {
            Transform camTrans = LuaObjs.GetTransform(camObjID);
            if (camTrans != null)
            {
                Transform trans = LuaObjs.GetTransform(luaObjID);
                if (trans != null)
                {
                    localToWorldMat.SetTRS(trans.position, camTrans.rotation, Vector3.one);
                    worldPos = localToWorldMat.MultiplyPoint(localPos);
                    return true;
                }
                else
                {
                    worldPos = localPos;
                    return false;
                }
            }
            else
            {
                worldPos = localPos;
                return false;
            }
        }

        void Update()
        {
            TempUpdate tu = null;
            Vector3 worldPos;
            bool doo = false;
            float time = Time.time;
            for (int i = 0, count = updateList.Count; i < count; i++)
            {
                tu = updateList[i];
                if (tu != null && tu.objID >= 0)
                {
                    if (time - tu.beginTime <= lifeTime)
                    {
                        bool v = CalculateWorldPos(tu.objID, tu.camObjID, tu.localCenter, out worldPos);
                        if (v)
                        {
                            doo = true;
                            for (uint j = tu.begin; j < tu.end; j++)
                            {
                                uint indexPos = j * BC_VERTEX_EACH_BOARD;
                                mCenters[indexPos] = worldPos;
                                mCenters[indexPos + 1] = worldPos;
                                mCenters[indexPos + 2] = worldPos;
                                mCenters[indexPos + 3] = worldPos;
                            }
                        }
                    }
                    else
                        tu.objID = -1;
                }
            }

            if (doo)
            {
                mMesh.vertices = mCenters;
            }
        }
    }
}
