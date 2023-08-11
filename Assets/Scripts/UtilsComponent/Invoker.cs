using System;
using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Invoke delay on a PersistentSingleton object
    /// </summary>
    public class Invoker : LazyPersistentSingletonMono<Invoker>
    {
        public static Coroutine InvokeDelay(float delay, Action action) => I.InvokeDelay(delay, action);

        public static Coroutine InvokeDelayRealtime(float delay, Action action) => I.InvokeDelayRealtime(delay, action);

        public static Coroutine InvokeDelayFrame(int frameCount, Action action) => I.InvokeDelayFrame(frameCount, action);

        public static void StopInvoke(Coroutine routine)
        {
            if (IsSingletonAlive)
                I.StopCoroutine(routine);
        }

        public static void StopAllInvoke()
        {
            if (IsSingletonAlive)
                I.StopAllCoroutines();
        }
    }
}

