using UnityEngine;

namespace NFramework
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private void Awake() => DontDestroyOnLoad(gameObject);
    }
}