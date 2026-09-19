using UnityEngine;

namespace NFramework.UI
{
    /// <summary>
    /// Adjusts the RectTransform anchors to conform to the device safe area (notches, home bars, cutouts).
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [SerializeField] private bool _conformX = true;
        [SerializeField] private bool _conformY = true;

        private RectTransform _panel;
        private Rect _lastSafeArea = Rect.zero;
        private Vector2Int _lastScreenSize = Vector2Int.zero;
        private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

        private void OnEnable()
        {
            if (!_panel)
            {
                _panel = GetComponent<RectTransform>();
            }

            Refresh();
        }

        private void OnRectTransformDimensionsChange()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (!_panel)
                return;

            Rect safeArea = Screen.safeArea;

            if (safeArea != _lastSafeArea
                || Screen.width != _lastScreenSize.x
                || Screen.height != _lastScreenSize.y
                || Screen.orientation != _lastOrientation)
            {
                _lastScreenSize.x = Screen.width;
                _lastScreenSize.y = Screen.height;
                _lastOrientation = Screen.orientation;

                ApplySafeArea(safeArea);
            }
        }

        private void ApplySafeArea(Rect r)
        {
            _lastSafeArea = r;

            if (!_conformX)
            {
                r.x = 0;
                r.width = Screen.width;
            }

            if (!_conformY)
            {
                r.y = 0;
                r.height = Screen.height;
            }

            if (Screen.width > 0 && Screen.height > 0)
            {
                Vector2 anchorMin = r.position;
                Vector2 anchorMax = r.position + r.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                // Fix for devices returning NaN or negative coordinates during startup
                if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
                {
                    _panel.anchorMin = anchorMin;
                    _panel.anchorMax = anchorMax;
                    _panel.offsetMin = Vector2.zero;
                    _panel.offsetMax = Vector2.zero;
                }
            }
        }
    }
}
