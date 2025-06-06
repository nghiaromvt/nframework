using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public abstract class PrimeTweenAnimation : MonoBehaviour
    {
        public enum OnEnableActionType
        {
            None = 0,
            Play = 1,
            Restart = 2
        }
        
        public enum OnDisableActionType
        {
            None = 0,
            Pause = 1,
            Stop = 2,
            Complete = 3,
            StopAndResetValue = 4,
            DestroyGameObject = 5
        }
        
        [SerializeField, HorizontalGroup, LabelText("OnEnable")] protected OnEnableActionType _onEnableActionType;
        [SerializeField, HorizontalGroup, LabelText("OnDisable")] protected OnDisableActionType _onDisableActionType;
        
        [FoldoutGroup("Events")] public UnityEvent OnStart;
        [FoldoutGroup("Events")] public UnityEvent OnPlay;
        [FoldoutGroup("Events")] public UnityEvent OnComplete;
        
        protected Tween _tween;
        
        private void OnEnable()
        {
            switch (_onEnableActionType)
            {
                default:
                case OnEnableActionType.None:
                    break;
                case OnEnableActionType.Play:
                    Play();
                    break;
                case OnEnableActionType.Restart:
                    _tween.Stop();
                    Play();
                    break;
            }
        }
        
        private void OnDisable()
        {
            switch (_onDisableActionType)
            {
                default:
                case OnDisableActionType.None:
                    break;
                case OnDisableActionType.Pause:
                    if (_tween.isAlive) 
                        _tween.isPaused = true;
                    break;
                case OnDisableActionType.Stop:
                    _tween.Stop();
                    break;
                case OnDisableActionType.StopAndResetValue:
                    StopTweenAndResetValue();
                    break;
                case OnDisableActionType.Complete:
                    _tween.Complete();
                    break;
                case OnDisableActionType.DestroyGameObject:
                    _tween.Complete();
                    Destroy(gameObject);
                    break;
            }
        }
        
        private void Play()
        {
            if (_tween.isAlive)
            {
                if (_tween.isPaused)
                {
                    _tween.isPaused = false;
                    OnPlay?.Invoke();
                }
                
                return;
            }

            StartTween();
            _tween.OnComplete(() => OnComplete?.Invoke());
            OnStart?.Invoke();
            OnPlay?.Invoke();
        }
        
        public abstract void StartTween();

        public void StopTweenAndResetValue()
        {
            _tween.Stop();
            ResetValue();
        }
        
        protected abstract void ResetValue();
    }
}