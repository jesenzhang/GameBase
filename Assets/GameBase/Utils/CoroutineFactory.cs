using UnityEngine;
using System.Collections;
using System;

namespace GameBase
{
    public class CoroutineFactory
    {
        public static IEnumerator GenerateCoroutine(params object[] Functions)
        {
            if (Functions == null || Functions.Length == 0)
            {
                yield return null;

            }

            for (int i = 0, max = Functions.Length; i < max; i++)
            {
                if (Functions[i] is string)
                {
                    float seconds = 0;
                    if (float.TryParse((string)Functions[i], out seconds))
                    {
                        yield return new WaitForSeconds(seconds);
                    }
                }
                if (Functions[i] is int)
                {
                }
                if (Functions[i] is float)
                {
                    float seconds = (float)Functions[i];

                    yield return new WaitForSeconds(seconds);


                }
                if (Functions[i] is double)
                {

                }
                if (Functions[i] != null && Functions[i] is Action)
                {
                    ((Action)Functions[i])();
                    yield return null;
                }
                {
                    yield return Functions[i];
                }

            }
        }
    }
}