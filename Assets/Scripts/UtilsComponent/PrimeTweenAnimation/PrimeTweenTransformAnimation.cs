using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class PrimeTweenTransformAnimation : PrimeTweenAnimation
    {
        public enum TweenType
        {
            None = 0,
            Position = 1,
            LocalPosition = 2,
            Rotation = 3,
            LocalRotation = 4,
            Scale = 5
        }
        
        [Title("Settings")]
        [SerializeField, EnumToggleButtons] private TweenType _tweenType;
        [SerializeField, HideIf(nameof(_tweenType), Value = TweenType.None)] private TweenSettings<Vector3> _tweenSettings;
        
        public override void StartTween()
        {
            switch (_tweenType)
            {
                default:
                case TweenType.None:
                    break;
                case TweenType.Position:
                    _tween = Tween.Position(transform, _tweenSettings);
                    break;
                case TweenType.LocalPosition:
                    _tween = Tween.LocalPosition(transform, _tweenSettings);
                    break;
                case TweenType.Rotation:
                    _tween = Tween.Rotation(transform, _tweenSettings);
                    break;
                case TweenType.LocalRotation:
                    _tween = Tween.LocalRotation(transform, _tweenSettings);
                    break;
                case TweenType.Scale:
                    _tween = Tween.Scale(transform, _tweenSettings);
                    break;
            }
        }

        protected override void ResetValue()
        {
            switch (_tweenType)
            {
                default:
                case TweenType.None:
                    break;
                case TweenType.Position:
                    transform.position = _tweenSettings.startValue;
                    break;
                case TweenType.LocalPosition:
                    transform.localPosition = _tweenSettings.startValue;
                    break;
                case TweenType.Rotation:
                    transform.eulerAngles = _tweenSettings.startValue;
                    break;
                case TweenType.LocalRotation:
                    transform.localEulerAngles = _tweenSettings.startValue;
                    break;
                case TweenType.Scale:
                    transform.localScale = _tweenSettings.startValue;
                    break;
            }
        }
    }
}
