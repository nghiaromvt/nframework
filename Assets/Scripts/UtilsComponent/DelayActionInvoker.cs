using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public class DelayActionInvoker : MonoBehaviour
    {
        public UnityEvent OnInvoke;

        [SerializeField] private float _delay;
        [SerializeField] private bool _useRealtime;
        [SerializeField] private bool _once;

        private bool _invoked;
        
        private void OnEnable()
        {
            if (_once && _invoked) return;
            
            _invoked = true;
            
            if (_useRealtime)
                this.InvokeDelayRealtime(_delay, () => OnInvoke?.Invoke());
            else
                this.InvokeDelay(_delay, () => OnInvoke?.Invoke());
        }

        // For call from inspector
        public void DestroySelf() => Destroy(gameObject);
    }
}
