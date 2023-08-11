using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace NFramework
{
    [RequireComponent(typeof(RectTransform))]
    public class SwipeZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum ESwipeDirection { None, Up, Down, Left, Right }

        [Header("Settings")]
        [SerializeField] private float _minimalSwipeLength = 50f;
        [SerializeField] private float _maximumPressLength = 10f;

        [Header("Events")]
        [SerializeField] private UnityEvent OnZonePressed;
        [SerializeField] private UnityEvent<ESwipeDirection> OnZoneSwiped;

        private Vector2 _firstTouchPosition;
        private float _length;
        private Vector2 _destination;
        private ESwipeDirection _swipeDirection;
        private float _angle;
        private Vector2 _deltaSwipe;
        private float _lastPointerUpAt;

        public void OnPointerDown(PointerEventData data) => _firstTouchPosition = Input.mousePosition;

        public void OnPointerUp(PointerEventData data)
        {
            if (Time.frameCount == _lastPointerUpAt)
                return;

            _destination = Input.mousePosition;
            _deltaSwipe = _destination - _firstTouchPosition;
            _length = _deltaSwipe.magnitude;

            // if the swipe has been long enough
            if (_length > _minimalSwipeLength)
            {
                _angle = MathUtils.AngleBetween2D(_deltaSwipe, Vector2.right);
                _swipeDirection = AngleToSwipeDirection(_angle);
                Swipe();
            }

            // if it's just a press
            if (_length < _maximumPressLength)
                Press();

            _lastPointerUpAt = Time.frameCount;
        }

        /// <summary>
        /// Determines a PossibleSwipeDirection out of an angle in degrees. 
        /// </summary>
        /// <returns>The to swipe direction.</returns>
        /// <param name="angle">Angle in degrees.</param>
        private ESwipeDirection AngleToSwipeDirection(float angle)
        {
            if ((angle < 45) || (angle >= 315))
                return ESwipeDirection.Right;
            if ((angle >= 45) && (angle < 135))
                return ESwipeDirection.Up;
            if ((angle >= 135) && (angle < 225))
                return ESwipeDirection.Left;
            if ((angle >= 225) && (angle < 315))
                return ESwipeDirection.Down;

            return ESwipeDirection.None;
        }

        private void Swipe() => OnZoneSwiped?.Invoke(_swipeDirection);

        private void Press() => OnZonePressed?.Invoke();
    }
}