using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleStoppedEventInvoker : MonoBehaviour
    {
        public UnityEvent EventOnParticleSystemStopped;

        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private bool _once = true;

        private bool _invoked;
        
        private void OnValidate()
        {
            _particleSystem ??= GetComponent<ParticleSystem>();
        }

        private void OnParticleSystemStopped()
        {
            if (_once && _invoked) return;

            _invoked = true;
            EventOnParticleSystemStopped?.Invoke();
        }
        
        // For call from inspector
        public void DestroySelf() => Destroy(gameObject);
    }
}
