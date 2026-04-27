using System;
using System.Collections;
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
            Debug.LogError(FormatMessage(exception, context, color), context);
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

            var text = message switch
            {
                IDictionary dict => FormatDictionary(dict),
                IList list => FormatList(list),
                _ => message.ToString()
            };

            var sb = new StringBuilder(text);

            if (context != null)
                sb.Insert(0, $"[{context.name}] ");

            if (color != null)
            {
                sb.Insert(0, $"<color=#{color.Value.ToHtmlStringRGB()}>");
                sb.Append("</color>");
            }

            return sb.ToString();
        }
        
        public static string FormatList(IList list)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(list[i]);
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static string FormatDictionary(IDictionary dict)
        {
            if (dict == null || dict.Count == 0)
                return "{}";

            var entries = new List<string>();
            foreach (DictionaryEntry kv in dict)
            {
                entries.Add($"{kv.Key}:{kv.Value}");
            }

            return "{ " + string.Join(", ", entries) + " }";
        }
    }
}
