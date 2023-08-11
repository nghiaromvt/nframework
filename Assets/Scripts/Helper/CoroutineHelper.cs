using System;
using System.Collections;
using UnityEngine;

namespace NFramework
{
    public static class CoroutineHelper
    {
        public static IEnumerator CRDelayAction(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        public static IEnumerator CRDelayActionRealtime(float delay, Action action)
        {
            yield return new WaitForSecondsRealtime(delay);
            action?.Invoke();
        }

        public static IEnumerator CRDelayFrameAction(int frameCount, Action action)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }
            action?.Invoke();
        }
    }
}


