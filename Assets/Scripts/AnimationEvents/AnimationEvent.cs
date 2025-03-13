using System;
using UnityEngine.Events;

namespace NFramework
{
    [Serializable]
    public class AnimationEvent
    {
        public string eventName;
        public UnityEvent OnAnimationEvent;
    }
}
