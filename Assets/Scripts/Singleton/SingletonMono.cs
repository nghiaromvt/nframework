using UnityEngine;

namespace NFramework
{
    public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T I { get; private set; }

        public static bool IsSingletonAlive => I != null;

        protected virtual void Awake()
        {
            if (I == null)
            {
                I = this as T;
            }
            else
            {
                Logger.LogError($"Duplicate singleton type of {typeof(T)}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (I == this)
                I = null;
        }
    }
}
