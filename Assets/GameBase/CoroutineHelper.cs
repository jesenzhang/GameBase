using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace GameBase
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static Queue<CoroutineHelper> pool = new Queue<CoroutineHelper>();
        private static List<CoroutineHelper> usingPool = new List<CoroutineHelper>();

        public static void CreateCoroutineHelper(IEnumerator cb)
        {
            if (cb == null)
                return;

            CoroutineHelper ch = GetHelper();
            if (ch == null)
            {
                GameObject go = new GameObject();
                Object.DontDestroyOnLoad(go);
                ch = go.AddComponent<CoroutineHelper>();
            }
            else
                ch.gameObject.SetActive(true);

            ch.BeginCoroutine(cb);
        }

        private static CoroutineHelper GetHelper()
        {
            if (pool.Count == 0)
                return null;
            return pool.Dequeue();
        }

        private static void DisposeHelper(CoroutineHelper ch)
        {
            if (ch == null)
                return;
            usingPool.Remove(ch);
            ch.gameObject.SetActive(false);
            pool.Enqueue(ch);
        }

        private void BeginCoroutine(IEnumerator cb)
        {
            if (cb == null)
                return;
            StartCoroutine(HelperCoroutine(cb));
        }

        private IEnumerator HelperCoroutine(IEnumerator cb)
        {
            yield return StartCoroutine(cb);

            DisposeHelper(this);
        }
    }
}