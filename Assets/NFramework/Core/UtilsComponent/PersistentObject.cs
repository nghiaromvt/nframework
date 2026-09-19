using UnityEngine;

namespace NFramework
{
    public class PersistentObject : MonoBehaviour
    {
        private void Awake() => DontDestroyOnLoad(gameObject);
    }
}