using System.Collections.Generic;

namespace NFramework
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Finds first key (if there's one) that matches the value set in parameters
        /// </summary>
        public static bool TryGetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, out TKey key)
        {
            key = default;
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
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
        public static List<TKey> GetKeysByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value)
        {
            List<TKey> keys = new List<TKey>();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                if (pair.Value.Equals(value))
                    keys.Add(pair.Key);
            }
            return keys;
        }

        public static bool IsNullOrEmpty<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return dictionary == null || dictionary.Count == 0;
        }
        
        /// <summary>
        /// Adds a key/value pair to the IDictionary&lt;TKey,TValue&gt; if the
        /// key does not already exist. Returns the new value, or the existing
        /// value if the key exists.
        /// </summary>
        public static TValue GetOrAddDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key) where TValue : new()
        {
            if (!source.ContainsKey(key)) source[key] = new TValue();
            return source[key];
        }
        /// <summary>
        /// Adds a key/value pair to the IDictionary&lt;TKey,TValue&gt; if the
        /// key does not already exist. Returns the new value, or the existing
        /// value if the key exists.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, 
            TKey key, TValue value)
        {
            source.TryAdd(key, value);
            return source[key];
        }
    }
}