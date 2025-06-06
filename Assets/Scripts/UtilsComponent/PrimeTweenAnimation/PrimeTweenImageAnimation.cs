using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(Graphic))]
    public class PrimeTweenImageAnimation : PrimeTweenAnimation
    {
        public enum TweenType
        {
            None = 0,
            Fade = 1,
            Color = 2,
            FillAmount = 3,
        }
        
        [SerializeField, ReadOnly] private Graphic _graphic;
        [SerializeField, EnumToggleButtons] private TweenType _tweenType;
        [SerializeField, ShowIf(nameof(_tweenType), Value = TweenType.Fade)] private TweenSettings<float> _fadeTweenSettings;
        [SerializeField, ShowIf(nameof(_tweenType), Value = TweenType.Color)] private TweenSettings<Color> _colorTweenSettings;
        [SerializeField, ShowIf(nameof(_tweenType), Value = TweenType.FillAmount)] private TweenSettings<float> _fillAmountTweenSettings;

        private void OnValidate()
        {
            if (!_graphic)
                _graphic = GetComponent<Graphic>();
        }
        
        public override void StartTween()
        {
            switch (_tweenType)
            {
                default:
                case TweenType.None:
                    break;
                case TweenType.Fade:
                    _tween = Tween.Alpha(_graphic, _fadeTweenSettings);
                    break;
                case TweenType.Color:
                    _tween = Tween.Color(_graphic, _colorTweenSettings);
                    break;
                case TweenType.FillAmount:
                    if (_graphic is Image image)
                        _tween = Tween.UIFillAmount(image, _fillAmountTweenSettings);
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
                    _graphic.color = _graphic.color.WithAlpha(_fadeTweenSettings.startValue);
                    break;
                case TweenType.Color:
                    _graphic.color = _colorTweenSettings.startValue;
                    break;
                case TweenType.FillAmount:
                    if (_graphic is Image image)
                        image.fillAmount = _fillAmountTweenSettings.startValue;
                    break;
            }
        }
    }
}