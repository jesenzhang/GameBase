
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GameBase
{
    public class GameTime : MonoBehaviour
    {
        private static float _realtime_F;
        public static float realtime_F
        {
            get { return _realtime_F; }
        }

        private static long _realtime_L;
        public static long realtime_L 
        {
            get { return _realtime_L; }
        }

        private static float _time_F;
        public static float time_F 
        {
            get { return _time_F; }
        }

        private static long _time_L;
        public static long time_L 
        {
            get { return _time_L; }
        }

        private static float _deltaTime_F;
        public static float deltaTime_F 
        {
            get { return _deltaTime_F; }
        }

        private static int _deltaTime_L;
        public static int deltaTime_L 
        {
            get { return _deltaTime_L; }
        }

        void Update() 
        {
            _realtime_F = Time.realtimeSinceStartup;
            _realtime_L = (long)(_realtime_F * 1000);

            _time_F = Time.time;
            _time_L = (long)(_time_F * 1000);

            _deltaTime_F = Time.deltaTime;
            _deltaTime_L = (int)(_deltaTime_F * 1000);
        }
    }
}
