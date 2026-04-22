using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace NFramework
{
    public static class NLogger
    {
        [Conditional("DEBUG_ENABLE"), Conditional("UNITY_EDITOR")]
        public static void Log(object message, Object context = null, Color? color = null)
        {
            Debug.Log(FormatMessage(message, context, color), context);
        }

        [Conditional("DEBUG_ENABLE"), Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message, Object context = null, Color? color = null)
        {
            Debug.LogWarning(FormatMessage(message, context, color), context);
        }

        [Conditional("DEBUG_ENABLE"), Conditional("UNITY_EDITOR"), Conditional("ENABLE_ERROR_LOG")]
        public static void LogError(object message, Object context = null, Color? color = null)
        {
            Debug.LogError(FormatMessage(message, context, color), context);
        }
        
        [Conditional("DEBUG_ENABLE"), Conditional("UNITY_EDITOR"), Conditional("ENABLE_ERROR_LOG")]
        public static void LogException(Exception exception, Object context = null, Color? color = null)
        {
            Debug.Log(FormatMessage(exception.Message, context, color), context);
        }

        [Conditional("DEBUG_ENABLE"), Conditional("UNITY_EDITOR")]
        public static void LogAssert(bool condition, object message, Object context = null, Color? color = null)
        {
            if (!condition)
                LogError(message, context, color);
        }

        public static string FormatMessage(object message, Object context, Color? color)
        {
            if (message == null)
                return string.Empty;

            var sb = new StringBuilder(message.ToString());

            if (context != null)
                sb.Insert(0, $"[{context.name}] ");

            if (color != null)
            {
                sb.Insert(0, $"<color=#{color.Value.ToHtmlStringRGB()}>");
                sb.Append("</color>");
            }

            return sb.ToString();
        }
        
        public static string FormatList(List<int> list)
        {
            if (list == null || list.Count == 0)
                return "[]";

            return "[" + string.Join(", ", list) + "]";
        }

        public static string FormatDictionary(Dictionary<int, int> dict)
        {
            if (dict == null || dict.Count == 0)
                return "{}";

            List<string> entries = new();
            foreach (var kv in dict)
            {
                entries.Add($"{kv.Key}:{kv.Value}");
            }

            return "{ " + string.Join(", ", entries) + " }";
        }
    }
}
