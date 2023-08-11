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

        public ParticleSystem ParticleSystem { get; private set; }

        private void Awake() => ParticleSystem = GetComponent<ParticleSystem>();

        private void OnParticleCollision(GameObject other) => EventOnParticleCollision?.Invoke(other);

        private void OnParticleSystemStopped() => EventOnParticleSystemStopped?.Invoke();

        private void OnParticleTrigger() => EventOnParticleTrigger?.Invoke();

        private void OnParticleUpdateJobScheduled() => EventOnParticleUpdateJobScheduled?.Invoke();
    }
}
