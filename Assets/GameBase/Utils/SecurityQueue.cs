using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace GameBase
{
    public class SecurityQueue<T>
    {
        private Queue<T> queue = null;
        private System.Object obj = null;

        public SecurityQueue()
        {
            
            queue = new Queue<T>();
            obj = new System.Object();
        }

        public void Enqueue(T t)
        {
            lock (obj)
            {
                
                queue.Enqueue(t);
            }
        }

        public bool Contains(T t)
        {
            lock (obj)
            {
                return queue.Contains(t);
            }
        }
        public void Clear()
        {
            lock (obj)
            {
                queue.Clear();
            }
        }


        public int Count
        {
            get { return queue.Count; }
        }

        //public bool Dequeue(ref T t)
        public bool Dequeue(out T t)
        {
            lock (obj)
            {
                if (queue.Count <= 0)
                {
                    t = default(T);
                    return false;
                }
                t = queue.Dequeue();
                return true;
            }
        }
    }
}
