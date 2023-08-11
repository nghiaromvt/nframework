using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(Button))]
    public class HoldToScaleButton : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IPointerEnterHandler
    {
        [SerializeField] private Vector3 _scaleValue = new Vector3(1.2f, 1.2f, 1.2f);
        [SerializeField] private Transform _scaleTarget;

        private Vector3 _baseScale;
        private bool _onHold;
        private Button _button;

        private void Awake()
        {
            _baseScale = transform.localScale;
            _button = GetComponent<Button>();

            if (!_scaleTarget)
                _scaleTarget = transform;
        }

        private void OnDisable()
        {
            _onHold = false;
            transform.localScale = _baseScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _onHold = true && _button.interactable;
            if (_onHold)
                transform.localScale = _scaleValue;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_onHold)
                transform.localScale = _baseScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _onHold = false;
            transform.localScale = _baseScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_onHold)
                transform.localScale = _scaleValue;
        }
    }
}