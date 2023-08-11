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
                if (!IsSingletonAlive)
                {
                    _i = FindObjectOfType<T>();
                    if (_i == null)
                    {
                        GameObject obj = new GameObject($"[{typeof(T).Name}]");
                        _i = obj.AddComponent<T>();
                    }
                }
                return _i;
            }
        }

        public static bool IsSingletonAlive => _i != null;

        protected virtual void Awake()
        {
            if (I != this)
            {
                Logger.LogError($"An instance of {typeof(T).Name} already existed! Destroy this!");
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
