using UnityEngine;

namespace vietlabs.fr2
{
    internal static class FR2_UIExtensions
    {
        internal static Color WithAlpha(this Color c, float alpha)
        {
            return new Color(c.r, c.g, c.b, alpha);
        }

        internal static Color WithBrightness(this Color c, float brightness)
        {
            return new Color(c.r * brightness, c.g * brightness, c.b * brightness, c.a);
        }

        internal static Color ToGrayscale(this Color c)
        {
            float gray = c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
            return new Color(gray, gray, gray, c.a);
        }

        internal static Color LerpTo(this Color from, Color to, float t, bool easeInOut = false)
        {
            if (easeInOut)
            {
                t = t * t * (3f - 2f * t);
            }
            return Color.Lerp(from, to, t);
        }

        internal static bool IsFocused(this FR2_TabView tabView, int index)
        {
            return tabView != null && tabView.current == index;
        }

        internal static bool IsFocusedAny(this FR2_TabView tabView, params int[] indices)
        {
            if (tabView == null) return false;
            foreach (int index in indices)
            {
                if (tabView.current == index) return true;
            }
            return false;
        }

        internal static bool SwitchTo(this FR2_TabView tabView, int index)
        {
            if (tabView == null || index < 0) return false;
            tabView.current = index;
            return true;
        }

        internal static int GetTabCount(this FR2_TabView tabView)
        {
            return tabView?.labels?.Length ?? 0;
        }

        internal static bool IsValidTabIndex(this FR2_TabView tabView, int index)
        {
            return tabView != null && index >= 0 && index < tabView.GetTabCount();
        }

        internal static Color GetTextColor(this FR2_Theme theme, bool isActive = false)
        {
            return isActive ? theme.IconActiveColor : theme.IconColor;
        }

        internal static Color GetBackgroundColor(this FR2_Theme theme, bool isSuccess = false, bool isWarning = false, bool isError = false)
        {
            if (isError) return theme.ErrorColor;
            if (isWarning) return theme.WarningColor;
            if (isSuccess) return theme.SuccessColor;
            return GUI.backgroundColor;
        }

        internal static Color GetContrastingColor(this Color backgroundColor)
        {
            float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;
            return luminance > 0.5f ? Color.black : Color.white;
        }

        internal static GUIStyle WithTextColor(this GUIStyle style, Color color)
        {
            var newStyle = new GUIStyle(style);
            newStyle.normal.textColor = color;
            newStyle.active.textColor = color;
            newStyle.focused.textColor = color;
            newStyle.hover.textColor = color;
            return newStyle;
        }

        internal static GUIStyle WithBackground(this GUIStyle style, Texture2D background)
        {
            var newStyle = new GUIStyle(style);
            newStyle.normal.background = background;
            return newStyle;
        }

        internal static GUIStyle WithAlignment(this GUIStyle style, TextAnchor alignment)
        {
            var newStyle = new GUIStyle(style);
            newStyle.alignment = alignment;
            return newStyle;
        }

    }
}