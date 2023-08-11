using System;
using System.Collections.Generic;

namespace NFramework
{
    public static class EnumHelper
    {
        public static Dictionary<int, string> ConvertToDictionaryKeyInt<T>() where T : struct
        {
            var dict = new Dictionary<int, string>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                var key = (int)value;
                dict.Add(key, value.ToString());
            }
            return dict;
        }

        public static Dictionary<string, int> ConvertToDictionaryKeyString<T>() where T : struct
        {
            var dict = new Dictionary<string, int>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
            {
                var key = (int)value;
                dict.Add(value.ToString(), key);
            }
            return dict;
        }

        public static List<int> ConvertToListInt<T>() where T : struct
        {
            var result = new List<int>();
            var values = Enum.GetValues(typeof(T));
            foreach (var value in values)
                result.Add((int)value);

            return result;
        }
    }
}


