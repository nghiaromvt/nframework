using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public class AutoInvokeWithDelay : MonoBehaviour
    {
        public UnityEvent OnInvoke;

        [SerializeField] private float _delay;
        [SerializeField] private bool _useRealtime;

        private void OnEnable()
        {
            if (_useRealtime)
                this.InvokeDelayRealtime(_delay, () => OnInvoke?.Invoke());
            else
                this.InvokeDelay(_delay, () => OnInvoke?.Invoke());
        }
    }
}
