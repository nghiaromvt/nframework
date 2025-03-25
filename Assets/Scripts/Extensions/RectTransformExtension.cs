using UnityEngine;

namespace NFramework
{
    public static class RectTransformExtension
    {
        /// <summary>
        /// Set width of RectTransform with sizeDelta.x
        /// </summary>
        public static void SetWidth(this RectTransform rectTransform, float width) 
            => rectTransform.sizeDelta = rectTransform.sizeDelta.WithX(width);

        /// <summary>
        /// Set height of RectTransform with sizeDelta.y
        /// </summary>
        public static void SetHeight(this RectTransform rectTransform, float height) 
            => rectTransform.sizeDelta = rectTransform.sizeDelta.WithY(height);
        
        public static void SetAnchoredPositionX(this RectTransform rectTransform, float x) 
            => rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithX(x);

        public static void SetAnchoredPositionY(this RectTransform rectTransform, float y) 
            => rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithY(y);

        public static void OffsetAnchoredPositionX(this RectTransform rectTransform, float x) 
            => rectTransform.anchoredPosition = rectTransform.anchoredPosition.OffsetX(x);

        public static void OffsetAnchoredPositionY(this RectTransform rectTransform, float y) 
            => rectTransform.anchoredPosition = rectTransform.anchoredPosition.OffsetY(y);
        
        /// <summary>
        /// Adds the specified amount as Vector2 to the source RectTransform's both
        /// anchors.
        /// </summary>
        public static RectTransform ShiftAnchor(this RectTransform source, Vector2 delta)
        {
            source.anchorMin += delta;
            source.anchorMax += delta;
            return source;
        }
		
        /// <summary>
        /// Adds the specified amount to the source RectTransform's both anchors.
        /// </summary>
        public static RectTransform ShiftAnchor(this RectTransform source, float x, float y) => source.ShiftAnchor(new Vector2(x, y));
        
        /// <summary>
        /// Gets the average of the sum of the source RectTransform's anchors.
        /// Effectively the parent-relative position of the RectTransform.
        /// </summary>
        public static Vector2 GetAnchorCenter(this RectTransform source) => (source.anchorMin + source.anchorMax) / 2;

        /// <summary>
        /// Gets the result of the source RectTransform's anchorMax subtracted by its
        /// anchorMin.
        /// Effectively the parent-relative size of the RectTransform.
        /// </summary>
        public static Vector2 GetAnchorDelta(this RectTransform source) => source.anchorMax - source.anchorMin;
        
        public static void SetSizeByWidth(this RectTransform rectTf, float width, float aspect)
        {
            rectTf.sizeDelta = new Vector2(width, width * aspect);
        }

        public static void SetSizeByHeight(this RectTransform rectTf, float height, float aspect)
        {
            rectTf.sizeDelta = new Vector2(height / aspect, height);
        }

        /// <summary>
        /// Sets the left offset of a rect transform to the specified value
        /// </summary>
        public static void SetLeft(this RectTransform rt, float left)
        {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }

        /// <summary>
        /// Sets the right offset of a rect transform to the specified value
        /// </summary>
        public static void SetRight(this RectTransform rt, float right)
        {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }

        /// <summary>
        /// Sets the top offset of a rect transform to the specified value
        /// </summary>
        public static void SetTop(this RectTransform rt, float top)
        {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        /// <summary>
        /// Sets the bottom offset of a rect transform to the specified value
        /// </summary>
        public static void SetBottom(this RectTransform rt, float bottom)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }

        public static void StretchFullParent(this RectTransform rt)
        {
            rt.transform.localPosition = Vector3.zero;
            rt.transform.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}