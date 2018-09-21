
using UnityEngine;
using Example;


//0-------2
//|	    / |
//|	  /   |
//| /	  |
//1-------3

namespace GameBase
{
    public partial class GPUBillboardBuffer_S : MonoBehaviour
    {
        private const int BC_VERTEX_EACH_BOARD = 4;
        private const int BC_INDICES_EACH_BOARD = 6;
        private const float BC_FONT_WIDTH = 0.6f;

        private static GPUBillboardBuffer_S instance = null;
        public static GPUBillboardBuffer_S Instance()
        {

            {

                if (instance == null)
                {
                    InitInstance(500);
                }
                return instance;
            }
        }


        private static GameObject mGameObject;
        private Mesh mMesh;
        private Material mMaterial;
        private MeshFilter mFilter;
        private MeshRenderer mRenderer;

        private Vector3[] mCenters;
        private Vector4[] mPosXYLifeScale;
        private Vector2[] mUv;
        //add by zxp
        private Vector2[] mUv2;

        private Color[] mColors;

        private uint mMaxBoardSize = 0;
        private uint mBoardIndex = 0;

        private int textureWidth = 0;
        private int textureHeight = 0;

        private static int standardWidth = 0;
        private static int standardHeight = 0;

        private float lifeTime = 1.5f;



        public static void InitInstance(uint maxSize)
        {
            if (instance == null)
            {
                mGameObject = new GameObject("GPUBillboardBuffer_S");
                Object.DontDestroyOnLoad(mGameObject);
                instance = mGameObject.AddComponent<GPUBillboardBuffer_S>();
                instance.Init(maxSize);
            }
        }

        public static void SetStandardWH(int w, int h)
        {
            standardWidth = w;
            standardHeight = h;
        }

        public void OnLeaveStage()
        {
            if (mMesh != null)
            {
                for (int i = 0; i < mMaxBoardSize; i++)
                {
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 1].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 2].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 3].Set(0, 0, -10000, 0);
                }
            }

            updateList.Clear();
        }

        public void Init(uint maxSize)
        {
            GPUBillboardBufferInit();

            SetupBillboard(maxSize);

            int count = (int)maxSize / 4;
            updateList = new System.Collections.Generic.List<TempUpdate>(count);
            for (int i = 0; i < count; i++)
            {
                updateList.Add(null);
            }
        }

        public void SetLayer(string layerName)
        {
            if (mGameObject != null)
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (Config.Detail_Debug_Log())
                    Debug.LogWarning("gpu billboard set layer->" + layer);
                if (layer >= 0)
                    mGameObject.layer = layer;
            }
        }

        private void GPUBillboardBufferInit()
        {
            mFilter = mGameObject.AddComponent<MeshFilter>();
            mRenderer = mGameObject.AddComponent<MeshRenderer>();
#if UNITY_EDITOR
		mRenderer.enabled = true;
#endif
            mMaterial = new Material(Shader.Find("Billboard/BillboardParticl_S_1"));
            mRenderer.material = mMaterial;

            mMesh = new Mesh();
            mFilter.mesh = mMesh;
        }

        //-------------------------------------------------------------------------------------------//
        public void SetTexture(Texture tex)
        {
            if (tex)
            {
                textureWidth = tex.width;
                textureHeight = tex.height;
            }

            mMaterial.SetTexture("_MainTex", tex);
        }

        internal void SetConfigs(float[] configs)
        {
            if (configs == null)
                return;
            if (configs.Length != 15)
                return;

            mMaterial.SetFloat("_Alpha_Acce", configs[0]);
            mMaterial.SetFloat("_Alpha_Value", configs[1]);
            mMaterial.SetFloat("_Alpha_Time", configs[2]);
            mMaterial.SetFloat("_Alpha_Limit", configs[3]);
            mMaterial.SetFloat("_Alpha_Start", configs[4]);

            mMaterial.SetFloat("_Scale_Acce", configs[5]);
            mMaterial.SetFloat("_Scale_Value", configs[6]);
            mMaterial.SetFloat("_Scale_Time", configs[7]);
            mMaterial.SetFloat("_Scale_Limit", configs[8]);
            mMaterial.SetFloat("_Scale_Start", configs[9]);

            mMaterial.SetFloat("_Up_Acce", configs[10]);
            mMaterial.SetFloat("_Up_Value", configs[11]);
            mMaterial.SetFloat("_Up_Time", configs[12]);
            mMaterial.SetFloat("_Up_Limit", configs[13]);
            mMaterial.SetFloat("_Up_Start", configs[14]);
        }

        //-------------------------------------------------------------------------------------------//
        public void SetupBillboard(uint maxBoardSize)
        {
            {
                mMaxBoardSize = maxBoardSize;

                mPosXYLifeScale = new Vector4[maxBoardSize * BC_VERTEX_EACH_BOARD];
                for (int i = 0; i < mMaxBoardSize; i++)
                {
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 1].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 2].Set(0, 0, -10000, 0);
                    mPosXYLifeScale[i * BC_VERTEX_EACH_BOARD + 3].Set(0, 0, -10000, 0);
                }

                mCenters = new Vector3[maxBoardSize * BC_VERTEX_EACH_BOARD];

                mUv = new Vector2[maxBoardSize * BC_VERTEX_EACH_BOARD];
                mUv2 = new Vector2[maxBoardSize * BC_VERTEX_EACH_BOARD];

                mColors = new Color[maxBoardSize * BC_VERTEX_EACH_BOARD];

                mMesh.vertices = mCenters;
                mMesh.tangents = mPosXYLifeScale;
                mMesh.colors = mColors;
                mMesh.uv = mUv;
                mMesh.uv = mUv2;

                {
                    int[] Indices = new int[maxBoardSize * BC_INDICES_EACH_BOARD];
                    for (int i = 0; i < maxBoardSize; ++i)
                    {
                        int index = i * BC_INDICES_EACH_BOARD;
                        int vertex = i * BC_VERTEX_EACH_BOARD;
                        Indices[index] = vertex;
                        Indices[index + 1] = vertex + 1;
                        Indices[index + 2] = vertex + 2;

                        Indices[index + 3] = vertex + 2;
                        Indices[index + 4] = vertex + 1;
                        Indices[index + 5] = vertex + 3;
                    }
                    mMesh.triangles = Indices;
                }
            }
            mMesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(100000, 100000, 100000));
        }
        //-------------------------------------------------------------------------------------------//
        public void DisplayNumber(int luaObjID, int camObjID, Example.PackItem[] items, int item_valid_num, Vector2 size, Vector3 localCenter, Color clr, bool haveScale, float offsetX)
        {
            if (item_valid_num <= 0)
                return;

            Vector3 center = localCenter;

            if (luaObjID >= 0)
            {
                bool re = CalculateWorldPos(luaObjID, camObjID, localCenter, out center);
                if (!re)
                    return;
            }

            float time = Time.timeSinceLevelLoad;

            int numLength = item_valid_num;
            Vector2 halfSize = new Vector2(size.x * 0.5f, size.y * 0.5f);

            Example.PackItem item;
            float leftBio = 0;
            for (int i = 0; i < numLength; ++i)
            {
                item = items[i];
                float sw = (float)item.Width / standardWidth;
                leftBio += size.x * sw;
            }

            leftBio *= -0.5f;

            int inthaveScale = 1;
            if (!haveScale)
            {
                inthaveScale = 0;
            }

            TempUpdate tu = GenTempUpdate();
            tu.objID = luaObjID;
            tu.camObjID = camObjID;
            tu.begin = mBoardIndex;
            tu.localCenter = localCenter;
            tu.beginTime = Time.time;

            float px = leftBio;
            for (int i = 0; i < numLength; ++i)
            {
                item = items[i];
                float fw = (float)item.Width / textureWidth;
                float fh = (float)item.Height / textureHeight;

                float sw = (float)item.Width / standardWidth;
                float sh = (float)item.Height / standardHeight;

                uint indexPos = mBoardIndex * BC_VERTEX_EACH_BOARD;

                float thh = 1 + sh * 0.5f;

                mPosXYLifeScale[indexPos].Set(px, halfSize.y * thh, time, inthaveScale);
                mPosXYLifeScale[indexPos + 1].Set(px, -halfSize.y * thh, time, inthaveScale);
                mPosXYLifeScale[indexPos + 2].Set(px + size.x * sw, halfSize.y * thh, time, inthaveScale);
                mPosXYLifeScale[indexPos + 3].Set(px + size.x * sw, -halfSize.y * thh, time, inthaveScale);

                px += size.x * sw;

                mCenters[indexPos] = center;
                mCenters[indexPos + 1] = center;
                mCenters[indexPos + 2] = center;
                mCenters[indexPos + 3] = center;

                mColors[indexPos] = clr;
                mColors[indexPos + 1] = clr;
                mColors[indexPos + 2] = clr;
                mColors[indexPos + 3] = clr;

                {
                    //计算UV//
                    Vector2 uvBegin = new Vector2((float)item.X / textureWidth, 1 - (float)item.Y / textureHeight);
                    mUv[indexPos] = uvBegin;
                    mUv[indexPos + 1].Set(uvBegin.x, uvBegin.y - fh);
                    mUv[indexPos + 2].Set(uvBegin.x + fw, uvBegin.y);
                    mUv[indexPos + 3].Set(uvBegin.x + fw, uvBegin.y - fh);
                }

                mUv2[indexPos].Set(offsetX, 0);
                mUv2[indexPos + 1].Set(offsetX, 0);
                mUv2[indexPos + 2].Set(offsetX, 0);
                mUv2[indexPos + 3].Set(offsetX, 0);


                mBoardIndex = ++mBoardIndex < mMaxBoardSize ? mBoardIndex : 0;
            }

            tu.end = mBoardIndex;

            mMesh.vertices = mCenters;
            mMesh.tangents = mPosXYLifeScale;
            mMesh.colors = mColors;
            mMesh.uv = mUv;
            mMesh.uv2 = mUv2;
        }
    }
}

