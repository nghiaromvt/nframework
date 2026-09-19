using System.Collections;
using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Invoke delay on a PersistentSingleton object
    /// </summary>
    public class Invoker : LazyPersistentSingletonMono<Invoker>
    {
        public static Coroutine Invoke(IEnumerator routine)
        {
            return HasInstance ? I.StartCoroutine(routine) : null;
        }

        public static void Stop(Coroutine coroutine)
        {
            if (HasInstance)
                I.StopCoroutine(coroutine);
        }

        public static void StopAll()
        {
            if (HasInstance)
                I.StopAllCoroutines();
        }
    }
}