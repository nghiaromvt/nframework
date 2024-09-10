using System;
using System.Globalization;
using System.Text.RegularExpressions;

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

        /// <summary>
		/// "CamelCaseString" => "Camel Case String"
		/// </summary>
		public static string SplitCamelCase(this string camelCaseString)
        {
            if (string.IsNullOrEmpty(camelCaseString))
                return camelCaseString;

            string camelCase = Regex.Replace(Regex.Replace(camelCaseString, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
            string firstLetter = camelCase.Substring(0, 1).ToUpper();

            if (camelCaseString.Length > 1)
            {
                string rest = camelCase.Substring(1);

                return firstLetter + rest;
            }

            return firstLetter;
        }

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

        public static bool IsNullOrEmpty(this string @this) => string.IsNullOrEmpty(@this);

        public static int ParseToInt(this string @this, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            return int.TryParse(@this, NumberStyles.Integer,
                CultureInfo.InvariantCulture.NumberFormat, out int result) ? result : defaultValue;
        }

        public static bool TryParseToInt(this string @this, out int result)
        {
            return int.TryParse(@this, NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat, out result);
        }

        public static float ParseToFloat(this string @this, float defaultValue = 0)
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            return float.TryParse(@this, NumberStyles.Float,
                CultureInfo.InvariantCulture.NumberFormat, out var result) ? result : defaultValue;
        }

        public static bool TryParseToFloat(this string @this, out float result)
        {
            return float.TryParse(@this, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out result);
        }

        public static T ParseToEnum<T>(this string @this, T defaultValue = default, bool ignoreCase = true) where T : struct
        {
            if (string.IsNullOrEmpty(@this))
                return defaultValue;

            T result;
            return Enum.TryParse(@this, ignoreCase, out result) ? result : defaultValue;
        }

        public static bool TryParseToEnum<T>(this string @this, out T result, bool ignoreCase = true) where T : struct
        {
            return Enum.TryParse(@this, ignoreCase, out result);
        }
    }
}
