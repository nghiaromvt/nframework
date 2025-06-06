using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PrimeTweenCanvasGroupAnimation : PrimeTweenAnimation
    {
        [Title("Settings")]
        [SerializeField, ReadOnly] private CanvasGroup _canvasGroup;
        [SerializeField] private TweenSettings<float> _tweenSettings;

        private void OnValidate()
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void StartTween() => _tween = Tween.Alpha(_canvasGroup, 0f, 1f);

        protected override void ResetValue() => _canvasGroup.alpha = _tweenSettings.startValue;
    }
}
