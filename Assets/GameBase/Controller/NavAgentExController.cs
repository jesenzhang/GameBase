using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameBase;
using Example;
using LuaInterface;
using System;
using UnityEngine.AI;

namespace GameBase.Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class NavAgentExController : MonoBehaviour
    {
        private float forecast_moveDistance = 0.0f;

        private float walkSpeed = 6;
        private float runSpeed = 12;
        private float backpedalSpeed = 3;

        private float turnEulerSpeed = 1440;
        private float curTurnEulerSpeed = 1440;
        private float jumpPower = 8;
        private float slopeLimit = 60;
        private float fallThreshold = 10;

        private bool _controllable = true;
        public bool controllable
        {
            get { return _controllable; }
        }

        private bool _grounded = false;
        public bool grounded
        {
            get { return _grounded; }
        }

        private float speedPercent = 1;
        private float _speed = 0;
        private Vector3 _velocity = Vector3.zero;
        private float _fall_start = 0;
        private Vector3 _last_position = Vector3.zero;
        private Vector3 _wanted_position = Vector3.zero;
        private Vector3 _last_wanted_position = Vector3.zero;

        private Transform m_transform;
        private CharacterController _controller;

        private Transform select;

        private bool canBack = true;

        private NavMeshPath path;
        private int pathIndex = 0;
        private Vector3 moveDir;
        private float curPathDistance = 0;
        private float pathNodeMoveCost = 0;

        private bool _isMovingToTarget;
        private bool _isNavJumping;
        private Vector3 linkStart;
        private Vector3 linkEnd;
        public bool isMovingToTarget
        {
            get { return _isMovingToTarget; }
        }

        private List<Vector2i> coordPath;
        private Vector3 curPathDest;
        private Vector3 prevDestPosition;
        private bool isPathMoving = false;

        private bool forceMove = false;

        private Vector3 Y_UP = new Vector3(0, 100, 0);
        private Vector3 Y_DOWN = new Vector3(0, -100, 0);
        private int surfaceMask;
        private int layerMask;

        private const float ORIGINY = -10000000;
        private float prevCalculateY = ORIGINY;
        private float stepY = 0.05f;
        private float maxPY = 1;

        private Vector3 verticalRayHigh = new Vector3(0, 0.5f, 0);
        private Vector3 verticalRayLow = new Vector3(0, 5 - 1.8f, 0);

        private float prevIX = 0;
        private float prevIY = 0;
        private bool ixyGetPoint = false;
        private Vector3 prevIXYPos;


        private const byte STATUS_IDLE = 1;
        private const byte STATUS_MOVE = 2;
        private const byte STATUS_WALK = 3;

        private const byte EVENT_MOVESTOP = 1;
        private const byte EVENT_FALLDOWN = 2;
        private const byte EVENT_FALLDOWNOVER = 3;
        private const byte EVENT_FALLDOWNDAMAGE = 4;
        private const byte EVENT_JUMP = 5;
        private const byte EVENT_MOVETOTARGET = 6;
        private const byte EVENT_SHIFTLEFT = 7;
        private const byte EVENT_SHIFTRIGHT = 8;
        private const byte EVENT_FINDPATHFAILED = 9;
        private const byte EVENT_FINDPATHSUCCESS = 10;


        private bool _isjoystickMove = false;
        private float _joystickX = 0;
        private float _joystickY = 0;

        private StateBase lifeState;

        private bool shiftLeft = false;
        private bool shiftRight = false;

        private float cameraSin = 0;
        private float cameraCos = 0;

        public enum ControlMode
        {
            IDLE = 0,
            RUNNING = 1,
            WALKING = 2,
        }

        private LuaFunction luaStateChangeCall = null;
        private int luaStateCallParam = -1;
        private LuaFunction luaEventCall = null;
        private int luaEventCallParam = -1;

        private ControlMode _controlMode = ControlMode.IDLE;
        private ControlMode controlMode
        {
            get { return _controlMode; }
            set
            {
                _controlMode = value;
                if (lifeState != null)
                {
                    if (_controlMode == ControlMode.IDLE)
                        lifeState.SetState(STATUS_IDLE);
                    else if (_controlMode == ControlMode.RUNNING)
                        lifeState.SetState(STATUS_MOVE);
                    else if (_controlMode == ControlMode.WALKING)
                        lifeState.SetState(STATUS_WALK);
                }


                if (luaStateChangeCall != null)
                {
                    if (_controlMode == ControlMode.IDLE)
                        LuaManager.CallFunc_VX(luaStateChangeCall, luaStateCallParam, STATUS_IDLE);
                    else if (_controlMode == ControlMode.RUNNING)
                        LuaManager.CallFunc_VX(luaStateChangeCall, luaStateCallParam, STATUS_MOVE);
                    else if (_controlMode == ControlMode.WALKING)
                        LuaManager.CallFunc_VX(luaStateChangeCall, luaStateCallParam, STATUS_WALK);
                }
            }
        }

        void Awake()
        {
            m_transform = transform;
            _controller = GetComponent<CharacterController>();

            _controller.slopeLimit = slopeLimit;
            _controller.center = new Vector3(0, 1, 0);
            _controller.radius = 0.24f;

            _wanted_position = m_transform.position;

            layerMask = 1 << LayerMask.NameToLayer("Surface") | 1 << LayerMask.NameToLayer("Wall");
            surfaceMask = 1 << LayerMask.NameToLayer("Surface");

            NavMeshHit meshHit;
            if (NavMesh.SamplePosition(transform.position, out meshHit, 10, NavMesh.AllAreas))
            {
                transform.position = meshHit.position;
            }
            path = new NavMeshPath();
        }

        void Start()
        {
        }

        public void SetLuaStateChangeCall(LuaFunction call, int param)
        {
            luaStateChangeCall = call;
            luaStateCallParam = param;
        }

        public void SetLuaEventCall(LuaFunction call, int param)
        {
            luaEventCall = call;
            luaEventCallParam = param;
        }

        public void Reset()
        {
            _controllable = true;
            _grounded = false;

            speedPercent = 1;
            _speed = 0;
            _velocity = Vector3.zero;
            _fall_start = 0;
            _last_position = Vector3.zero;
            _wanted_position = Vector3.zero;
            _last_wanted_position = Vector3.zero;
            select = null;

            canBack = true;

            if (path != null)
                path.ClearCorners();
            pathIndex = 0;
            curPathDistance = 0;
            _isMovingToTarget = false;
            _isNavJumping = false;

            prevCalculateY = GetLayerCollisionY(m_transform.position) + 0.1f;
        }

        public void SetRunSpeed(float v)
        {
            runSpeed = v;
        }

        public void SetWalkSpeed(float v)
        {
            walkSpeed = v;
        }

        public float GetControllerHeight()
        {
            if (_controller == null)
                return 0;

            return _controller.height;
        }

        public void SetControllerCenter(Vector3 center)
        {
            if (_controller == null)
                return;

            _controller.center = center;
        }

        public void SetControllerRadius(float r)
        {
            if (_controller == null)
                return;

            _controller.radius = r;
        }

        public void SetControllerHeight(float h)
        {
            if (_controller == null)
                return;

            _controller.height = h;
        }

        public void SetSpeedPercent(float v)
        {
            speedPercent = v;
        }

        public void SetControllerStateBase(StateBase s)
        {
            lifeState = s;
        }

        public void SetBackpedalSpeed(float v)
        {
            backpedalSpeed = v;
        }

        public void SetForceMove()
        {
            forceMove = true;
        }

        public void SetRayInfo(float verticalheight, float verticallen, float horizontalLen)
        {
            verticalRayHigh = new Vector3(0, verticalheight, 0);
            verticalRayLow = new Vector3(0, verticallen - verticalheight, 0);
        }

        public void SetStepY(float y)
        {
            stepY = y;
        }

        public void SetMaxPY(float max)
        {
            maxPY = max;
        }

        public float GetSurfaceY(Vector3 pos)
        {
            RaycastHit hit;

            if (Physics.Linecast(pos + Y_UP, pos + Y_DOWN, out hit, surfaceMask))
            {
                return hit.point.y;
            }

            return ORIGINY;
        }

        public float GetSurfaceY(Vector3 pos, float y_up, float y_down)
        {
            RaycastHit hit;

            Vector3 up = new Vector3(0, y_up, 0);
            Vector3 down = new Vector3(0, y_down, 0);
            if (Physics.Linecast(pos + up, pos + down, out hit, surfaceMask))
            {
                return hit.point.y;
            }

            return ORIGINY;
        }

        public float GetLayerCollisionY(Vector3 pos)
        {
            RaycastHit hit;

            if (Physics.Linecast(pos + Y_UP, pos + Y_DOWN, out hit, layerMask))
            {
                return hit.point.y;
            }

            return ORIGINY;
        }

        public float GetLayerCollisionY(Vector3 pos, float y_up, float y_down)
        {
            RaycastHit hit;

            Vector3 up = new Vector3(0, y_up, 0);
            Vector3 down = new Vector3(0, y_down, 0);
            if (Physics.Linecast(pos + up, pos + down, out hit, layerMask))
            {
                return hit.point.y;
            }

            return ORIGINY;
        }


        private float SurfaceY(Vector3 pos)
        {
            RaycastHit hit;

            pos.y = prevCalculateY;

            if (Physics.Linecast(pos + verticalRayHigh, pos - verticalRayLow, out hit, layerMask))
            {
                if (prevCalculateY == ORIGINY)
                {
                    prevCalculateY = hit.point.y;
                    return prevCalculateY;
                }
                float py = hit.point.y - prevCalculateY;

                if (py > stepY)
                {
                    if (hit.point.y > prevCalculateY)
                        prevCalculateY += stepY;
                    else
                        prevCalculateY -= stepY;
                }
                else
                    prevCalculateY = hit.point.y;

                return prevCalculateY;
            }
            else
            {
                return prevCalculateY;
            }
        }

        private float CalculateY(Vector3 pos)
        {
            RaycastHit hit;

            pos.y = prevCalculateY;

            if (Physics.Linecast(pos + verticalRayHigh, pos - verticalRayLow, out hit, layerMask))
            {
                if (prevCalculateY == ORIGINY)
                {
                    prevCalculateY = hit.point.y;
                    return prevCalculateY;
                }
                float py = hit.point.y - prevCalculateY;

                if (py >= maxPY || py <= -maxPY)
                {
                    return ORIGINY;
                }

                if (py > stepY)
                {
                    if (hit.point.y > prevCalculateY)
                        prevCalculateY += stepY;
                    else
                        prevCalculateY -= stepY;
                }
                else
                    prevCalculateY = hit.point.y;

                return prevCalculateY;
            }
            else
            {
                return ORIGINY;
            }
        }

        public void SetCameraAngle(float angley)
        {
            float r = (180 - angley) / 180 * Mathf.PI;
            cameraSin = Mathf.Sin(r);
            cameraCos = Mathf.Cos(r);
        }

        private Transform camTrans = null;

        public int Move(float ix, float iy)
        {
            iy = -iy;
            if (camTrans == null)
            {
                if (Camera.main != null)
                    camTrans = Camera.main.transform;
            }

            if (camTrans == null)
                return -200;
            Vector3 dir = camTrans.forward;

            float r = (float)Math.Sqrt(ix * ix + iy * iy);
            float sin = ix / r;
            float cos = iy / r;

            float ox, oz;
            GameMath.VectorRotate(dir.x, dir.z, sin, cos, out ox, out oz);
            moveDir.x = ox;
            moveDir.y = 0;
            moveDir.z = oz;

            moveDir.Normalize();

            Vector3 selfPos = m_transform.position;
            Vector3 pos = selfPos + moveDir * 2;
            bool doo = true;
            NavMeshHit meshHit;
            if (!NavMesh.Raycast(selfPos, pos, out meshHit, NavMesh.AllAreas))
            {
            }
            else
            {
                if (prevIX == ix && prevIY == iy)
                {
                    if (ixyGetPoint)
                        return 0;
                }

                if (NavMesh.SamplePosition(pos, out meshHit, 2, NavMesh.AllAreas))
                {
                    pos = meshHit.position;
                    float dis = Vector3.Distance(selfPos, pos);
                    if(dis < 0.5f)
                    {
                        if (NavMesh.FindClosestEdge(pos, out meshHit, NavMesh.AllAreas))
                        {
                            pos = meshHit.position;
                        }
                        else
                            return 0;
                    }
                }
                else
                    doo = false;
            }

            if (prevIX != ix || prevIY != iy)
            {
                prevIX = ix;
                prevIY = iy;
                ixyGetPoint = doo;
            }

            if (doo)
            {
                _joystickX = ix;
                _joystickY = -iy;
                _isjoystickMove = true;
                prevIXYPos = pos;
                int re = MoveTo(pos);
                if (re < 0)
                    ixyGetPoint = false;

                return re;
            }
            else
            {
                Stop();
                return 0;
            }
        }

        public void SetTo(Vector3 pos)
        {
            m_transform.position = pos;
        }

        public int MoveTo(Vector3 pos, bool once = false)
        {
            path.ClearCorners();
            if (NavMesh.CalculatePath(m_transform.position, pos, NavMesh.AllAreas, path))
            {
                pathIndex = 1;
                if (path.corners.Length <= 1)
                    pathIndex = 0;
                
                prevDestPosition = m_transform.position;
                curPathDistance = (prevDestPosition - path.corners[pathIndex]).sqrMagnitude;
                if (!grounded)
                    path.corners[pathIndex].y = prevDestPosition.y;
                moveDir = (path.corners[pathIndex] - prevDestPosition).normalized;
                _isMovingToTarget = true;
                if (luaEventCall != null)
                    LuaManager.CallFunc_VX(luaEventCall, luaEventCallParam, EVENT_FINDPATHSUCCESS);
                pathNodeMoveCost = 0;

                controlMode = ControlMode.RUNNING;
            }
            else
            {
                if (!once)
                {
                    NavMeshHit hit;
                    if (NavMesh.Raycast(m_transform.position, pos, out hit, NavMesh.AllAreas))
                    {
                        if (hit.mask == 0)
                        {
                            if (NavMesh.FindClosestEdge(m_transform.position, out hit, NavMesh.AllAreas))
                            {
                                return MoveTo(hit.position, true);
                            }
                        }
                        else
                        {
                            return MoveTo(hit.position, true);
                        }
                    }
                }

                if (luaEventCall != null)
                    LuaManager.CallFunc_VX(luaEventCall, luaEventCallParam, EVENT_FINDPATHFAILED);
                return -1000;
            }

            return 0;
        }

        private Vector3 PositionByLogicCoord(int x, int y)
        {
            return GameCommon.PositionByLogicCoord(x, y);
        }

        private const float HEXAGON_BASEWIDTH = 0.25f;
        private const float HEXAGON_R = 2 * HEXAGON_BASEWIDTH * 1.732f / 3;

        private Vector3 HexagonPositionByLogicCoord(int x, int y)
        {
            Vector3 v3 = new Vector3();
            if (y % 2 == 1)
                v3.x = x / 2 + 0.25f + 0.5f * (x % 2) - HEXAGON_BASEWIDTH;
            else
                v3.x = x / 2 + 0.25f + 0.5f * (x % 2);

            v3.z = y * HEXAGON_R * 1.5f + HEXAGON_R;

            v3.x -= 23;
            v3.z -= 1;

            return v3;
        }

        public Vector3 MoveTo(List<int> path)
        {
            if (path == null || path.Count == 0)
                return Vector3.zero;
            List<Vector2i> list = new List<Vector2i>();
            for (int i = 0, count = path.Count; i < count; i += 2)
            {
                Vector2i v2i = new Vector2i();
                v2i.X = path[i];
                v2i.Y = path[i + 1];
                list.Add(v2i);
            }
            coordPath = list;

            pathIndex = 0;

            Vector2i coord = coordPath[pathIndex];
            curPathDest = PositionByLogicCoord(coord.X, coord.Y);
            curPathDest.y = m_transform.position.y;
            prevDestPosition = m_transform.position;
            if (pathIndex == (coordPath.Count - 1))
                curPathDistance = (prevDestPosition - curPathDest).sqrMagnitude + forecast_moveDistance;
            else
                curPathDistance = (prevDestPosition - curPathDest).sqrMagnitude;
            moveDir = (curPathDest - prevDestPosition).normalized;
            isPathMoving = true;
            controlMode = ControlMode.RUNNING;

            return PositionByLogicCoord(list[list.Count - 1].X, list[list.Count - 1].Y);
        }

        public void Stop()
        {
            StopNav();
        }

        private void StopNav()
        {
            if (!isMovingToTarget)
                return;

            _isjoystickMove = false;
            _joystickX = 0;
            _joystickY = 0;
            pathNodeMoveCost = 0;
            ixyGetPoint = false;

            curPathDistance = 0;
            _isMovingToTarget = false;
            if (controlMode == ControlMode.IDLE)
                return;
            pathIndex = 0;
            path.ClearCorners();

            controlMode = ControlMode.IDLE;
            if (!forceMove)
                _controller.Move(Vector3.zero);

            if (luaEventCall != null)
                LuaManager.CallFunc_VX(luaEventCall, luaEventCallParam, EVENT_MOVESTOP);
        }

        public void SetCanBack(bool _canback)
        {
            canBack = _canback;
        }

        private void ApplyRotation(float deltaTime)
        {
            if (curTurnEulerSpeed <= 0)
                return;

            float targetAngleY;
            if (moveDir.x > 0)
                targetAngleY = Vector3.Angle(moveDir, Vector3.forward);
            else
                targetAngleY = Vector3.Angle(moveDir, Vector3.back) + 180;

            float m = targetAngleY % 360;
            float n = m_transform.eulerAngles.y % 360;
            if (m < 0)
                m += 360;
            if (n < 0)
                n += 360;
            if (m - n < -180)
                m += 360;
            float dy = (m - n);

            float step = deltaTime * curTurnEulerSpeed;

            if (Mathf.Abs(dy) < step)
            {
                m_transform.eulerAngles = new Vector3(0, targetAngleY, 0);
            }
            else
            {
                int j = (dy > 0 && dy < 180) ? 1 : -1;
                m_transform.Rotate(new Vector3(0, j * step, 0), Space.Self);
            }
        }

        public void LogNavMeshAgent()
        {
            Debug.LogError("-----------------log navmesh begin-----------------------");
            Debug.LogError("path->" + pathIndex + "^" + path.corners.Length);
            Debug.LogError("------------------log navmesh end-----------------------");
        }

        private void SetPosition(Vector3 pos)
        {
            return;
        }

        public void LogStatusInfo()
        {
            Debug.Log("----------------------log status-----------------------------");
            Debug.Log("ismovetotarget->" + isMovingToTarget);
            Debug.Log("ground->" + grounded);
            Debug.Log("velcity->" + _velocity);
            Debug.Log("position->" + m_transform.position);
            Debug.Log("controller is ground->" + _controller.isGrounded);
            Debug.Log("---------------------log status end--------------------------");
        }

        private const float JUMPTIME = 0.6f;
        private float curJumpTime = 0;
        private Vector3 navJumpDir;

        private void UpdateStopNav(int len)
        {
            ixyGetPoint = false;
            if (_isjoystickMove)
            {
                if (_joystickX != 0 || _joystickY != 0)
                {
                    Move(_joystickX, _joystickY);
                    return;
                }
            }

            if (pathIndex >= len)
                pathIndex = len - 1;
            if (pathIndex >= 0)
            {
            }
            StopNav();
            if (lifeState != null)
                lifeState.SetEvent(EVENT_MOVETOTARGET, null);

            if (luaEventCall != null)
                LuaManager.CallFunc_VX(luaEventCall, luaEventCallParam, EVENT_MOVETOTARGET);
        }

        public void OnDisable()
        {
            prevUpdateTime = 0;
        }

        public void OnEnable()
        {
            prevUpdateTime = 0;
        }

        private float prevUpdateTime = 0;
        void FixedUpdate()
        {
            if (controlMode == ControlMode.IDLE)
            {
            }
            else
            {
                if (speedPercent <= 0)
                    return;
                float deltaTime = Time.deltaTime;
                if (_isMovingToTarget)
                {
                    int len = path.corners.Length;
                    if (pathIndex < 0)
                    {
                        UpdateStopNav(len);
                        _speed = 0;
                        return;
                    }

                    float dis = (m_transform.position - prevDestPosition).sqrMagnitude;

                    if (pathIndex <= len - 1)
                    {
                        if((dis - curPathDistance) >= -0.3f)
                        {
                            Vector3 vec = path.corners[pathIndex];
                            SetPosition(vec);

                            pathIndex++;
                            pathNodeMoveCost = 0;
                            if (pathIndex < len)
                            {
                                if (!grounded)
                                    path.corners[pathIndex].y = m_transform.position.y;
                                moveDir = (path.corners[pathIndex] - m_transform.position).normalized;
                                curPathDistance = (m_transform.position - path.corners[pathIndex]).sqrMagnitude;
                                prevDestPosition = path.corners[pathIndex - 1];
                                _speed = runSpeed;
                            }
                            else
                            {
                                pathIndex--;
                                if (pathIndex < 0)
                                    pathIndex = 0;
                                UpdateStopNav(len);
                                _speed = 0;
                            }
                        }
                        else
                        {
                            _speed = runSpeed;
                        }
                    }
                    else if (dis >= curPathDistance)
                    {
                        UpdateStopNav(len);
                        _speed = 0;
                    }
                    else
                    {
                    }

                    pathNodeMoveCost += deltaTime;
                    if (pathNodeMoveCost > 50)
                    {
                        UpdateStopNav(len);
                        return;
                    }

                    ApplyRotation(deltaTime);
                }
                else
                {
                    _speed = runSpeed;
                }

                if (_speed == 0)
                {
                    return;
                }

                _speed = _speed * speedPercent;

                _velocity = moveDir * _speed;

                float t_dis = (m_transform.position + (_velocity * deltaTime) - prevDestPosition).sqrMagnitude;
                if (t_dis > curPathDistance)
                {
                    float c_dis = (m_transform.position - prevDestPosition).sqrMagnitude;
                    float dis = Mathf.Sqrt(curPathDistance) - Mathf.Sqrt(c_dis);
                    float c_speed = dis / deltaTime;
                    _velocity = moveDir * c_speed;
                }

                if (forceMove)
                    m_transform.position += _velocity * deltaTime;
                else
                {
                    _velocity.y = -50;

                    _controller.Move(_velocity * deltaTime);
                }
            }
        }

        public void FallingDamage(float fall_distance)
        {
            if (lifeState != null)
                lifeState.SetEvent(EVENT_FALLDOWNDAMAGE, new System.Object[] { fall_distance });
            if (luaEventCall != null)
                LuaManager.CallFunc_VX(luaEventCall, luaEventCallParam, EVENT_FALLDOWNDAMAGE);
        }
    }
}