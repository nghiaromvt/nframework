using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(Button))]
    public class HoldToTriggerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float _holdDuration = 0.15f;

        private Button _button;
        private bool _onHold = false;
        private float _holdingTime = 0;

        private void Awake() => _button = GetComponent<Button>();

        private void Update()
        {
            if (!_onHold || !_button.interactable)
                return;

            _holdingTime += Time.deltaTime;

            if (_holdingTime >= _holdDuration)
            {
                _holdingTime -= _holdDuration;
                _button.onClick.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _holdingTime = 0;
            _onHold = true && _button.interactable;
        }

        public void OnPointerUp(PointerEventData eventData) => _onHold = false;
    }
}