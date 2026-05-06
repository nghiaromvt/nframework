using PrimeTween;
using UnityEngine;
using Sirenix.OdinInspector;

namespace NFramework
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PrimeTweenSpriteRendererAnimation : PrimeTweenAnimation
    {
        public enum TweenType
        {
            None = 0,
            Fade = 1,
            Color = 2,
        }
        
        [SerializeField, ReadOnly] private SpriteRenderer _spriteRenderer;
        [SerializeField, EnumToggleButtons] private TweenType _tweenType;
        [SerializeField, ShowIf(nameof(_tweenType), Value = TweenType.Fade)] private TweenSettings<float> _fadeTweenSettings;
        [SerializeField, ShowIf(nameof(_tweenType), Value = TweenType.Color)] private TweenSettings<Color> _colorTweenSettings;

        private void OnValidate()
        {
            if (!_spriteRenderer)
                _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        public override void StartTween()
        {
            switch (_tweenType)
            {
                default:
                case TweenType.None:
                    break;
                case TweenType.Fade:
                    _tween = Tween.Alpha(_spriteRenderer, _fadeTweenSettings);
                    break;
                case TweenType.Color:
                    _tween = Tween.Color(_spriteRenderer, _colorTweenSettings);
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
                case TweenType.Fade:
                    _spriteRenderer.color = _spriteRenderer.color.WithAlpha(_fadeTweenSettings.startValue);
                    break;
                case TweenType.Color:
                    _spriteRenderer.color = _colorTweenSettings.startValue;
                    break;
            }
        }
    }
}