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
            base.OnStateEnter(animator, stateInfo, layerIndex);
            _hasTriggered = false;
            _receiver = animator.GetComponent<AnimationEventReceiver>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            
            if (_hasTriggered) return;

            if (stateInfo.normalizedTime >= triggerTime)
            {
                NotifyReceiver();
                _hasTriggered = true;
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            if (_hasTriggered) return;
            
            if (stateInfo.normalizedTime >= triggerTime)
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

