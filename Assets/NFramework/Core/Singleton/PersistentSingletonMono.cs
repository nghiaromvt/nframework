using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Non-lazy persistent singleton MonoBehaviour that persists across scene loads.
    /// Expected to be placed or initialized in the boot scene.
    /// </summary>
    public class PersistentSingletonMono<T> : SingletonMono<T> where T : MonoBehaviour
    {
        protected override void Awake()
        {
            base.Awake();

            if (I == this && Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}
