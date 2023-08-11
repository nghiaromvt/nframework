using UnityEngine;
using UnityEngine.UI;

namespace NFramework
{
    public static class ImageExtension
    {
        public static void SetSizeByWidth(this Image img, float width)
        {
            if (img.sprite == null)
                return;
            var sprite = img.sprite;
            float aspect = sprite.bounds.size.y / sprite.bounds.size.x;
            img.rectTransform.sizeDelta = new Vector2(width, width * aspect);
        }

        public static void SetSizeByHeight(this Image img, float height)
        {
            if (img.sprite == null)
                return;
            var sprite = img.sprite;
            float aspect = sprite.bounds.size.y / sprite.bounds.size.x;
            img.rectTransform.sizeDelta = new Vector2(height / aspect, height);
        }

        public static void SetAlpha(this Image img, float alpha)
        {
            img.color = img.color.WithAlpha(alpha);
        }
    }
}