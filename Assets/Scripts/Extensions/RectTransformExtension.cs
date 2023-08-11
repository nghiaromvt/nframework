using UnityEngine;

namespace NFramework
{
    public static class RectTransformExtension
    {
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