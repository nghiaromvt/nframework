using UnityEngine;

namespace NFramework
{
    public class AnimationEventStateBehaviour : StateMachineBehaviour
    {
        public string eventName;
        [Range(0f, 1f)] public float triggerTime;

        private bool _hasTriggered;
        private AnimationEventReceiver _receiver;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _hasTriggered = false;
            _receiver = animator.GetComponent<AnimationEventReceiver>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            float currentTime = stateInfo.normalizedTime % 1f;

            if (!_hasTriggered && currentTime >= triggerTime)
            {
                NotifyReceiver();
                _hasTriggered = true;
            }
        }

        private void NotifyReceiver()
        {
            if (_receiver)
                _receiver.OnAnimationEventTriggered(eventName);
        }
    }
}

