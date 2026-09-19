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

        public static void AddOrUpdate<T>(this IDictionary<T, decimal> dic, T key, decimal val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, double> dic, T key, double val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, float> dic, T key, float val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, int> dic, T key, int val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, long> dic, T key, long val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, uint> dic, T key, uint val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static void AddOrUpdate<T>(this IDictionary<T, ulong> dic, T key, ulong val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = dic[key] + val;
        }

        public static V GetOrDefault<T, V>(this IDictionary<T, V> dic, T key, V defaultValue = default(V))
        {
            if (!dic.ContainsKey(key))
                dic[key] = defaultValue;
            return dic[key];
        }

        public static void AddOrSet<T, V>(this IDictionary<T, V> dic, T key, V val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = val;
        }

        public static void AddOrSetMax<T>(this IDictionary<T, decimal> dic, T key, decimal val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, double> dic, T key, double val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, float> dic, T key, float val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, int> dic, T key, int val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, long> dic, T key, long val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, uint> dic, T key, uint val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSetMax<T>(this IDictionary<T, ulong> dic, T key, ulong val)
        {
            if (!dic.ContainsKey(key))
                dic[key] = default;
            dic[key] = System.Math.Max(dic[key], val);
        }

        public static void AddOrSet<T, V>(this IDictionary<T, V> dic, IDictionary<T, V> otherDic)
        {
            foreach (var pair in otherDic)
            {
                dic.AddOrSet(pair.Key, pair.Value);
            }
        }
    }
}