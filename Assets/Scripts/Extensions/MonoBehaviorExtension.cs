using System;
using UnityEngine;

namespace NFramework
{
    public static class MonoBehaviorExtension
    {
        public static Coroutine InvokeDelay(this MonoBehaviour self, float delay, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayAction(delay, action));
        }

        public static Coroutine InvokeDelayRealtime(this MonoBehaviour self, float delay, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayRealtimeAction(delay, action));
        }

        public static Coroutine InvokeDelayFrame(this MonoBehaviour self, int frameCount, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayFrameAction(frameCount, action));
        }

        public static Coroutine InvokeDelayUntil(this MonoBehaviour self, Func<bool> predicate, Action action)
        {
            return self.StartCoroutine(CoroutineHelper.CRDelayUntilAction(predicate, action));
        }
    }
}


