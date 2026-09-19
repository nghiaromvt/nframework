using System;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    internal static class FR2_GUIExtensions
    {
        internal static bool DrawProgressBar(this string message, float progress, string progressText = null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(message, EditorStyles.boldLabel);
            
            Rect rect = FR2_Theme.Current.GetProgressBarRect();
            EditorGUI.ProgressBar(rect, progress, progressText ?? $"{(progress * 100):F1}%");
            
            EditorGUILayout.Space();
            return GUILayout.Button("Cancel", FR2_Theme.Current.CancelButtonHeight);
        }

        internal static bool DrawActionButton(this string text, GUILayoutOption height = null)
        {
            return GUILayout.Button(text, height ?? FR2_Theme.Current.ActionButtonHeight);
        }

        internal static bool DrawMiniButton(this string text, Rect rect)
        {
            return GUI.Button(rect, text, EditorStyles.miniButton);
        }

        internal static void DrawHelpBox(this string message, MessageType messageType = MessageType.Info)
        {
            if (!string.IsNullOrEmpty(message))
            {
                EditorGUILayout.HelpBox(message, messageType);
            }
        }

        internal static Rect GetRightRect(this Rect rect, float width)
        {
            return new Rect(rect.xMax - width, rect.y, width, rect.height);
        }

        internal static Rect GetLeftRect(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        internal static Rect GetBottomRect(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.yMax - height, rect.width, height);
        }

        internal static Rect ShrinkBottom(this Rect rect, float height)
        {
            rect.yMax -= height;
            return rect;
        }


        internal static bool DrawRefreshButton(this Rect rect, float buttonWidth = 24f)
        {
            Rect btnRect = rect.GetRightRect(buttonWidth);
            btnRect.yMin = rect.yMax;
            btnRect.height = 24f;
            return GUI.Button(btnRect, FR2_Icon.Refresh.image);
        }

        internal static void DrawColoredTexture(this Rect rect, Color color, Texture2D texture = null)
        {
            using (FR2_GUI.Color(color))
            {
                GUI.DrawTexture(rect, texture ?? EditorGUIUtility.whiteTexture);
            }
        }

        internal static bool DrawCopyButton(this string text, string tooltip = null)
        {
            GUIContent content = FR2_GUIContent.FromString(text, tooltip ?? "Click to copy");
            if (GUILayout.Button(content))
            {
                EditorGUIUtility.systemCopyBuffer = text;
                FR2_LOG.Log($"Copied: {text}");
                return true;
            }
            return false;
        }

        internal static bool DrawCopyButton(this string text, Rect rect, string tooltip = null)
        {
            GUIContent content = FR2_GUIContent.FromString(text, tooltip ?? "Click to copy");
            if (GUI.Button(rect, content))
            {
                EditorGUIUtility.systemCopyBuffer = text;
                FR2_LOG.Log($"Copied: {text}");
                return true;
            }
            return false;
        }

        internal static bool DrawToggleButton(this Rect rect, bool isActive, GUIContent content, Color? activeColor = null)
        {
            Color originalColor = GUI.color;
            Color originalContentColor = GUI.contentColor;
            
            if (isActive)
            {
                GUI.color = activeColor ?? FR2_Theme.Current.SuccessColor;
            }
            
            bool result = GUI.Button(rect, content, EditorStyles.toolbarButton);
            
            GUI.color = originalColor;
            GUI.contentColor = originalContentColor;
            
            return result;
        }

        internal static bool DrawTabButton(this string label, bool isSelected, GUIStyle style = null)
        {
            Color originalBgColor = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? FR2_Theme.Current.SelectionHighlight : originalBgColor;
            
            bool result = GUILayout.Toggle(isSelected, label, style ?? EditorStyles.toolbarButton);
            
            GUI.backgroundColor = originalBgColor;
            return result;
        }

        internal static void DrawWarningPanel(this string message, Texture2D icon = null, System.Action onClose = null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox(FR2_GUIContent.From(message, icon));
            if (onClose != null && GUILayout.Button("  x", EditorStyles.label, FR2_Theme.Current.CloseButtonWidth, FR2_Theme.Current.WarningCloseButtonHeight))
            {
                onClose();
            }
            EditorGUILayout.EndHorizontal();
        }

        internal static (Rect extracted, Rect remaining) ExtractLeft(this Rect rect, float width)
        {
            Rect extracted = new Rect(rect.x, rect.y, width, rect.height);
            Rect remaining = new Rect(rect.x + width, rect.y, rect.width - width, rect.height);
            return (extracted, remaining);
        }

        internal static (Rect extracted, Rect remaining) ExtractRight(this Rect rect, float width)
        {
            Rect extracted = new Rect(rect.xMax - width, rect.y, width, rect.height);
            Rect remaining = new Rect(rect.x, rect.y, rect.width - width, rect.height);
            return (extracted, remaining);
        }
    }
}
