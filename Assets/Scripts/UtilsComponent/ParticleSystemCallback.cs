using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemCallback : MonoBehaviour
    {
        public UnityEvent<GameObject> EventOnParticleCollision;
        public UnityEvent EventOnParticleSystemStopped;
        public UnityEvent EventOnParticleTrigger;
        public UnityEvent EventOnParticleUpdateJobScheduled;

        [SerializeField] private bool _constrainOnStoppedTriggerOneTime = true;

        public ParticleSystem ParticleSystem { get; private set; }

        private bool _triggered;

        private void Awake() => ParticleSystem = GetComponent<ParticleSystem>();

        private void OnEnable() => _triggered = false;

        private void OnParticleCollision(GameObject other) => EventOnParticleCollision?.Invoke(other);

        private void OnParticleSystemStopped()
        {
            if (_constrainOnStoppedTriggerOneTime && _triggered)
                return;

            _triggered = true;
            EventOnParticleSystemStopped?.Invoke();
        }

        private void OnParticleTrigger() => EventOnParticleTrigger?.Invoke();

        private void OnParticleUpdateJobScheduled() => EventOnParticleUpdateJobScheduled?.Invoke();
    }
}
