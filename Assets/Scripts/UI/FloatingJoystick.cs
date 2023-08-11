using UnityEngine.EventSystems;

namespace NFramework
{
    public class FloatingJoystick : Joystick
    {
        protected override void Start()
        {
            base.Start();
            _background.gameObject.SetActive(false);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            _background.gameObject.SetActive(true);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            _background.gameObject.SetActive(false);
            base.OnPointerUp(eventData);
        }
    }
}
