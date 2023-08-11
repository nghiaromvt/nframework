using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework
{
    [RequireComponent(typeof(RectTransform))]
    public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum EAxisOptions { Both, Horizontal, Vertical }

        [Header("Bindings")]
        [Tooltip("The background's RectTransform component.")]
        [SerializeField] protected RectTransform _background = null;
        [Tooltip("The handle's RectTransform component.")]
        [SerializeField] protected RectTransform _handle = null;

        [Header("Settings")]
        [Tooltip("The distance the visual handle can move from the center of the joystick.")]
        [SerializeField] private float _handleRange = 1;
        [Tooltip("The distance away from the center input has to be before registering.")]
        [SerializeField] private float _deadZone = 0;
        [Tooltip("Which axes the joystick uses.")]
        [SerializeField] private EAxisOptions _axisOption = EAxisOptions.Both;
        [Tooltip("Snap the horizontal input to a whole value.")]
        [SerializeField] private bool _snapX = false;
        [Tooltip("Snap the vertical input to a whole value.")]
        [SerializeField] private bool _snapY = false;
        [Tooltip("Should hide background and handle.")]
        [SerializeField] private bool _hideVisual = false;

        private Vector2 _input = Vector2.zero;
        private RectTransform _baseRect;
        private Canvas _canvas;
        private Camera _cam;

        public float Horizontal => (_snapX) ? SnapFloat(_input.x, EAxisOptions.Horizontal) : _input.x;
        public float Vertical => (_snapY) ? SnapFloat(_input.y, EAxisOptions.Vertical) : _input.y;
        public Vector2 Direction => new Vector2(Horizontal, Vertical);

        public float HandleRange
        {
            get => _handleRange;
            set { _handleRange = Mathf.Abs(value); }
        }

        public float DeadZone
        {
            get => _deadZone;
            set { _deadZone = Mathf.Abs(value); }
        }

        public EAxisOptions AxisOption { get => _axisOption; set { _axisOption = value; } }
        public bool SnapX { get => _snapX; set { _snapX = value; } }
        public bool SnapY { get => _snapY; set { _snapY = value; } }

        protected virtual void Start()
        {
            HandleRange = _handleRange;
            DeadZone = _deadZone;
            _baseRect = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
                Debug.LogError("The Joystick is not placed inside a canvas", this);

            Vector2 center = new Vector2(0.5f, 0.5f);
            _background.pivot = center;
            _handle.anchorMin = center;
            _handle.anchorMax = center;
            _handle.pivot = center;
            _handle.anchoredPosition = Vector2.zero;

            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _cam = _canvas.worldCamera;
            else
                _cam = null;

            if (_hideVisual)
            {
                _background.GetComponent<Image>().SetAlpha(0);
                _handle.GetComponent<Image>().SetAlpha(0);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position = RectTransformUtility.WorldToScreenPoint(_cam, _background.position);
            Vector2 radius = _background.sizeDelta / 2;
            _input = (eventData.position - position) / (radius * _canvas.scaleFactor);
            FormatInput();
            HandleInput(_input.magnitude, _input.normalized, radius, _cam);
            _handle.anchoredPosition = _input * radius * _handleRange;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            _handle.anchoredPosition = Vector2.zero;
        }

        protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > _deadZone)
            {
                if (magnitude > 1)
                    _input = normalised;
            }
            else
                _input = Vector2.zero;
        }

        private void FormatInput()
        {
            if (_axisOption == EAxisOptions.Horizontal)
                _input = new Vector2(_input.x, 0f);
            else if (_axisOption == EAxisOptions.Vertical)
                _input = new Vector2(0f, _input.y);
        }

        private float SnapFloat(float value, EAxisOptions snapAxis)
        {
            if (value == 0)
                return value;

            if (_axisOption == EAxisOptions.Both)
            {
                float angle = Vector2.Angle(_input, Vector2.up);
                if (snapAxis == EAxisOptions.Horizontal)
                {
                    if (angle < 22.5f || angle > 157.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                else if (snapAxis == EAxisOptions.Vertical)
                {
                    if (angle > 67.5f && angle < 112.5f)
                        return 0;
                    else
                        return (value > 0) ? 1 : -1;
                }
                return value;
            }
            else
            {
                if (value > 0)
                    return 1;
                if (value < 0)
                    return -1;
            }
            return 0;
        }

        protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
        {
            Vector2 localPoint = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_baseRect, screenPosition, _cam, out localPoint))
            {
                Vector2 pivotOffset = _baseRect.pivot * _baseRect.sizeDelta;
                return localPoint - (_background.anchorMax * _baseRect.sizeDelta) + pivotOffset;
            }
            return Vector2.zero;
        }
    }
}
