using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NFramework
{
    public static class Logger
    {
        [Conditional("DEVELOPMENT")]
        public static void Log(object message, Object context = null, Color? color = null)
        {
            Debug.Log(FormatMessage(message, context, color), context);
        }

        [Conditional("DEVELOPMENT")]
        public static void LogWarning(object message, Object context = null, Color? color = null)
        {
            Debug.LogWarning(FormatMessage(message, context, color), context);
        }

        [Conditional("DEVELOPMENT")]
        public static void LogError(object message, Object context = null, Color? color = null)
        {
            Debug.LogError(FormatMessage(message, context, color), context);
        }

        [Conditional("DEVELOPMENT")]
        public static void LogAssert(bool condition, object message, Object context = null, Color? color = null)
        {
            if (!condition)
                LogError(message, context, color);
        }

        private static string FormatMessage(object message, Object context, Color? color)
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
    }
}
