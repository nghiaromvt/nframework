using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(Button))]
    public class UIButtonFeedback : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerExitHandler,
        IPointerEnterHandler
    {
        #region Fields

        [SerializeField] private Transform _targetTransform;

        private Button _button;
        private Vector3 _initialScale;
        private Tween _tween;
        private int _activePointerId = int.MinValue;

        #endregion

        #region Properties

        protected virtual float ScaleInMultiplier => 0.9f;
        protected virtual float DurationIn => 0.1f;
        protected virtual float DurationOut => 0.15f;
        protected virtual float Amplitude => 1.5f;
        protected virtual Ease EaseIn => Ease.OutQuad;
        protected virtual bool UseUnscaledTime => true;

        #endregion

        #region Unity Lifecycle Functions

        private void OnValidate()
        {
            if (_targetTransform == null)
            {
                _targetTransform = transform;
            }
        }

        private void Awake()
        {
            _button = GetComponent<Button>();

            if (_targetTransform == null)
            {
                _targetTransform = transform;
            }

            _initialScale = _targetTransform.localScale;
            if (_initialScale == Vector3.zero)
            {
                _initialScale = Vector3.one;
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClick);
            _activePointerId = int.MinValue;

            if (_tween.isAlive)
            {
                _tween.Stop();
            }

            if (_targetTransform != null)
            {
                _targetTransform.localScale = _initialScale;
            }
        }

        #endregion

        #region Pointer Event Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.IsInteractable())
            {
                return;
            }

            if (_activePointerId != int.MinValue)
            {
                return;
            }

            _activePointerId = eventData.pointerId;
            AnimateScale(_initialScale * ScaleInMultiplier, DurationIn, EaseIn);
            OnPointerDownFeedback();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            AnimateScale(_initialScale, DurationOut, Easing.Overshoot(Amplitude));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            if (!_button.IsInteractable())
            {
                return;
            }

            AnimateScale(_initialScale * ScaleInMultiplier, DurationIn, EaseIn);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != _activePointerId)
            {
                return;
            }

            _activePointerId = int.MinValue;
            AnimateScale(_initialScale, DurationOut, Easing.Overshoot(Amplitude));
        }

        #endregion

        #region Feedback & Click Hooks

        private void HandleButtonClick()
        {
            if (!_button.IsInteractable())
            {
                return;
            }

            OnClick();
        }

        protected virtual void OnPointerDownFeedback()
        {
        }

        protected virtual void OnClick()
        {
        }

        #endregion

        #region Private Methods

        private void AnimateScale(Vector3 targetScale, float duration, Easing easing)
        {
            if (_tween.isAlive)
            {
                _tween.Stop();
            }

            _tween = Tween.Scale(_targetTransform, targetScale, duration, easing, useUnscaledTime: UseUnscaledTime);
        }

        #endregion
    }
}
