using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NFramework
{
    public enum EStringMatchType
    {
        Exactly,
        Exactly_IgnoreCase,
        Contains,
        Contains_IgnoreCase
    }
    
    public static class StringExtension
    {
        /// <summary>
        /// Compare 2 string
        /// </summary>
        public static bool IsMatchWith(this string @this, string comparedString, EStringMatchType matchType = EStringMatchType.Exactly)
        {
            switch (matchType)
            {
                default:
                case EStringMatchType.Exactly:
                    return string.Equals(@this, comparedString, System.StringComparison.Ordinal);
                case EStringMatchType.Exactly_IgnoreCase:
                    return string.Equals(@this, comparedString, System.StringComparison.OrdinalIgnoreCase);
                case EStringMatchType.Contains:
                    return @this.Contains(comparedString);
                case EStringMatchType.Contains_IgnoreCase:
                    return @this.Contains(comparedString, true);
            }
        }
        
        /// <summary>
        /// Contains with ignore case param
        /// </summary>
        public static bool Contains(this string @this, string comparedString, bool ignoreCase)
        {
            if (ignoreCase)
            {
                comparedString = comparedString.ToLower();
                @this = @this.ToLower();
            }
            return @this.Contains(comparedString);
        }
        
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        
        /// <summary>
        /// "Camel case string" => "CamelCaseString" 
        /// </summary>
        public static string ToCamelCase(this string message) {
            message = message.Replace("-", " ").Replace("_", " ");
            message = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(message);
            message = message.Replace(" ", "");
            return message;
        }
        
        /// <summary>
        /// "CamelCaseString" => "Camel Case String"
        /// </summary>
        public static string SplitCamelCase(this string camelCaseString)
        {
            if (string.IsNullOrEmpty(camelCaseString)) return camelCaseString;

            string camelCase = Regex.Replace(Regex.Replace(camelCaseString, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
            string firstLetter = camelCase.Substring(0, 1).ToUpper();

            if (camelCaseString.Length > 1)
            {
                string rest = camelCase.Substring(1);

                return firstLetter + rest;
            }

            return firstLetter;
        }
        
        /// <summary>
        /// Surround string with "color" tag
        /// </summary>
        public static string Colored(this string message, Color color) => $"<color={color.ToHtmlStringRGB()}>{message}</color>";

        /// <summary>
        /// Surround string with "color" tag
        /// </summary>
        public static string Colored(this string message, string colorCode) => $"<color={colorCode}>{message}</color>";
        
        /// <summary>
        /// Surround string with "size" tag
        /// </summary>
        public static string Sized(this string message, int size) => $"<size={size}>{message}</size>";
		
        /// <summary>
        /// Surround string with "u" tag
        /// </summary>
        public static string Underlined(this string message) => $"<u>{message}</u>";

        /// <summary>
        /// Surround string with "b" tag
        /// </summary>
        public static string Bold(this string message) => $"<b>{message}</b>";

        /// <summary>
        /// Surround string with "i" tag
        /// </summary>
        public static string Italics(this string message) => $"<i>{message}</i>";
        
        public static string CapitalizeFirstChar(this string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            char firstChar = input[0];

            if (char.IsUpper(firstChar))
                return input;

            var chars = input.ToCharArray();
            chars[0] = char.ToUpper(firstChar);
            return new string(chars);
        }
        
        public static int ParseToInt(this string @this, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            return int.TryParse(@this, NumberStyles.Integer,
                CultureInfo.InvariantCulture.NumberFormat, out int result) ? result : defaultValue;
        }

        public static bool TryParseToInt(this string @this, out int result) => 
            int.TryParse(@this, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out result);

        public static float ParseToFloat(this string @this, float defaultValue = 0)
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            return float.TryParse(@this, NumberStyles.Float,
                CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : defaultValue;
        }

        public static bool TryParseToFloat(this string @this, out float result) => 
            float.TryParse(@this, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out result);

        public static T ParseToEnum<T>(this string @this, T defaultValue = default, bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            return Enum.TryParse(@this, ignoreCase, out T result) ? result : defaultValue;
        }

        public static bool TryParseToEnum<T>(this string @this, out T result, bool ignoreCase = true) where T : struct => 
            Enum.TryParse(@this, ignoreCase, out result);

        public static bool TryParseToColor(this string @this, out Color color) => 
            ColorUtility.TryParseHtmlString(@this, out color);
    }
}