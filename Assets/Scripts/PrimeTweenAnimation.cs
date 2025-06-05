using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class PrimeTweenAnimation : MonoBehaviour
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
            Rewind = 2,
            Kill = 3,
            KillAndComplete = 4,
            DestroyGameObject = 5
        }
        
        [SerializeField] private TweenSettings<Vector3> _tweenSettings;
        [SerializeField] private OnEnableActionType _onEnableActionType;
        [SerializeField] private OnDisableActionType _onDisableActionType;

        private Tween _tween;
        
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
                    break;
            }
        }

        public void Play()
        {
            if (_tween.isAlive) return;
            _tween = Tween.Position(transform, _tweenSettings);
        }

        public void Stop()
        {
            if (!_tween.isAlive) return;
            _tween.Stop();
        }
    }
}
