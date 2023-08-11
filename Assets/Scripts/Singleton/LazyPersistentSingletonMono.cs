using UnityEngine;

namespace NFramework
{
    public class LazyPersistentSingletonMono<T> : LazySingletonMono<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            if (I == this)
                DontDestroyOnLoad(gameObject);
        }
    }
}