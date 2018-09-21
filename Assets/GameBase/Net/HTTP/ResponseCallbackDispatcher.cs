using UnityEngine;
using System;
using System.Collections;

namespace GameBase
{
    public class ResponseCallbackDispatcher : MonoBehaviour
    {
        private static ResponseCallbackDispatcher singleton = null;
        private static GameObject singletonGameObject = null;
        private static object singletonLock = new object();

        public static ResponseCallbackDispatcher Singleton {
            get {
                return singleton;
            }
        }

        public Queue requests = Queue.Synchronized( new Queue() );

        public static void Init()
        {
            if ( singleton != null )
            {
                return;
            }

            lock( singletonLock )
            {
                if ( singleton != null )
                {
                    return;
                }

                singletonGameObject = new GameObject();
                GameObject.DontDestroyOnLoad(singletonGameObject);
                singleton = singletonGameObject.AddComponent< ResponseCallbackDispatcher >();
                singletonGameObject.name = "HTTPResponseCallbackDispatcher";
            }
        }

        public void Update()
        {
            while( requests.Count > 0 )
            {
                Request request = (Request)requests.Dequeue();
                if (request != null)
                {
                    request.completedCallback(request);
                }
                else
                {
                    
                }
                
            }
        }
    }
}
