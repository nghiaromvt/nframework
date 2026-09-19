using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private UnitySerializedDictionary<string, UnityEvent> _animationEvents = new();

        public UnityEvent GetAnimationEvent(string eventName) => _animationEvents.GetValueOrDefault(eventName);

        public void OnAnimationEventTriggered(string eventName)
        {
            if (_animationEvents.TryGetValue(eventName, out UnityEvent unityEvent))
                unityEvent?.Invoke();
        }

        public void RemoveAllListeners()
        {
            foreach (var unityEvent in _animationEvents.Values)
            {
                unityEvent.RemoveAllListeners();
            }
        }
    }
}
