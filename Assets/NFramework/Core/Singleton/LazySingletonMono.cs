using UnityEngine;

namespace NFramework
{
    public class LazySingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _i;

        public static T I
        {
            get
            {
                if (!HasInstance)
                {
                    if (Application.exitCancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    _i = FindAnyObjectByType<T>();
                    if (_i == null)
                    {
                        if (!Application.isPlaying)
                        {
                            return null;
                        }

                        GameObject obj = new GameObject($"[{typeof(T).Name}]");
                        _i = obj.AddComponent<T>();
                    }
                }
                return _i;
            }
        }

        public static bool HasInstance => _i != null;

        static LazySingletonMono()
        {
            SingletonResetRegistry.Register(ResetStatic);
        }

        private static void ResetStatic()
        {
            _i = null;
        }

        protected virtual void Awake()
        {
            if (I != this)
            {
                Debug.LogError($"An instance of {typeof(T).Name} already existed! Destroy this!");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_i == this)
                _i = null;
        }
    }
}
