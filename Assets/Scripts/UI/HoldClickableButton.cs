using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    public class HoldClickableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float _holdDuration;
        [SerializeField] private bool _ignoreTimeScale;
        [SerializeField] private Image _image;
        [SerializeField] private Sprite _pointerDownSprite;
        [SerializeField] private Sprite _pointerUpSprite;

        public event Action OnClicked;
        public event Action OnHoldClicked;

        private bool _isHoldingButton;
        private float _elapsedTime;

        private void Update() => ManageButtonInteraction();

        public void OnPointerDown(PointerEventData eventData) => ToggleHoldingButton(true);

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isHoldingButton)
                ManageButtonInteraction(true);
        }

        public void OnPointerEnter(PointerEventData eventData) { }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isHoldingButton)
                ToggleHoldingButton(false);
        }

        private void ToggleHoldingButton(bool isPointerDown)
        {
            _isHoldingButton = isPointerDown;

            if (isPointerDown)
                _elapsedTime = 0;

            _image.sprite = isPointerDown ? _pointerDownSprite : _pointerUpSprite;
        }

        private void ManageButtonInteraction(bool isPointerUp = false)
        {
            if (!_isHoldingButton)
                return;

            if (isPointerUp)
            {
                Click();
                return;
            }

            _elapsedTime += _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            var isHoldClickDurationReached = _elapsedTime > _holdDuration;

            if (isHoldClickDurationReached)
                HoldClick();
        }

        private void Click()
        {
            ToggleHoldingButton(false);
            OnClicked?.Invoke();
        }

        private void HoldClick()
        {
            ToggleHoldingButton(false);
            OnHoldClicked?.Invoke();
        }
    }
}
