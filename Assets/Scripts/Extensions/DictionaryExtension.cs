using System.Collections.Generic;

namespace NFramework
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Finds first key (if there's one) that matches the value set in parameters
        /// </summary>
        public static bool TryGetKeyByValue<T, W>(this Dictionary<T, W> dictionary, W value, out T key)
        {
            key = default;
            foreach (KeyValuePair<T, W> pair in dictionary)
            {
                if (pair.Value.Equals(value))
                {
                    key = pair.Key;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds first key (if there's one) that matches the value set in parameters
        /// </summary>
        public static List<T> GetKeysByValue<T, W>(this Dictionary<T, W> dictionary, W value)
        {
            List<T> keys = new List<T>();
            foreach (KeyValuePair<T, W> pair in dictionary)
            {
                if (pair.Value.Equals(value))
                    keys.Add(pair.Key);
            }
            return keys;
        }

        public static bool IsNullOrEmpty<T, K>(this Dictionary<T, K> dictionary)
        {
            return dictionary == null ? true : dictionary.Count == 0;
        }
    }
}

