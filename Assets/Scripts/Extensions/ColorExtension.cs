using UnityEngine;

namespace NFramework
{
    public static class ColorExtension
    {
        /// <summary>
        /// To string of "b5ff4f" format
        /// </summary>
        public static string ToHtmlStringRGB(this Color color) => ColorUtility.ToHtmlStringRGB(color);

        public static Color WithAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}