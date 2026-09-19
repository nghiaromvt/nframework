using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace vietlabs.fr2
{
    internal static class FR2_ObjectPool<T> where T : class, new()
    {
        private static readonly ConcurrentQueue<T> _objects = new ConcurrentQueue<T>();
        private static int _count = 0;
        private const int MAX_POOL_SIZE = 32;

        internal static T Get()
        {
            if (_objects.TryDequeue(out T item))
            {
                _count--;
                return item;
            }
            return new T();
        }

        internal static void Return(T item)
        {
            if (item != null && _count < MAX_POOL_SIZE)
            {
                _objects.Enqueue(item);
                _count++;
            }
        }
    }

    internal static class FR2_ListPool<T>
    {
        private static readonly ConcurrentQueue<List<T>> _lists = new ConcurrentQueue<List<T>>();
        private static int _count = 0;
        private const int MAX_POOL_SIZE = 16;

        internal static List<T> Get()
        {
            if (_lists.TryDequeue(out List<T> list))
            {
                _count--;
                return list;
            }
            return new List<T>();
        }

        internal static void Return(List<T> list)
        {
            if (list != null && _count < MAX_POOL_SIZE)
            {
                list.Clear();
                _lists.Enqueue(list);
                _count++;
            }
        }
    }

    internal static class FR2_HashSetPool<T>
    {
        private static readonly ConcurrentQueue<HashSet<T>> _sets = new ConcurrentQueue<HashSet<T>>();
        private static int _count = 0;
        private const int MAX_POOL_SIZE = 16;

        internal static HashSet<T> Get()
        {
            if (_sets.TryDequeue(out HashSet<T> set))
            {
                _count--;
                return set;
            }
            return new HashSet<T>();
        }

        internal static void Return(HashSet<T> set)
        {
            if (set != null && _count < MAX_POOL_SIZE)
            {
                set.Clear();
                _sets.Enqueue(set);
                _count++;
            }
        }
    }

    internal static class FR2_CollectionExtensions
    {
        internal static List<T> GetPooledList<T>()
        {
            return FR2_ListPool<T>.Get();
        }

        internal static void ReturnPooledList<T>(List<T> list)
        {
            FR2_ListPool<T>.Return(list);
        }

        internal static HashSet<T> GetPooledHashSet<T>()
        {
            return FR2_HashSetPool<T>.Get();
        }

        internal static void ReturnPooledHashSet<T>(HashSet<T> set)
        {
            FR2_HashSetPool<T>.Return(set);
        }

        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            var result = FR2_HashSetPool<T>.Get();
            if (collection == null) return result;

            foreach (T item in collection)
            {
                result.Add(item);
            }
            return result;
        }

        internal static bool HasItems<T>(this ICollection<T> collection)
        {
            return collection != null && collection.Count > 0;
        }

        internal static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        internal static int SafeCount<T>(this ICollection<T> collection)
        {
            return collection?.Count ?? 0;
        }

        internal static bool AddUnique<T>(this ICollection<T> collection, T item)
        {
            if (collection == null || collection.Contains(item)) return false;
            collection.Add(item);
            return true;
        }

        internal static int RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
        {
            if (list == null || predicate == null) return 0;
            
            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        #if UNITY_2021_2_OR_NEWER
        #else
        internal static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
        #endif
        
        internal static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;
            
            dictionary[key] = defaultValue;
            return defaultValue;
        }

        internal static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
        {
            if (dictionary.TryGetValue(key, out TValue value))
                return value;
            
            value = factory(key);
            dictionary[key] = value;
            return value;
        }

        internal static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) return false;
            dictionary[key] = value;
            return true;
        }

        internal static bool TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                return true;
            }
            return false;
        }

        internal static void SafeExecute(this Action action, string context = null)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(context) 
                    ? $"Exception in SafeExecute: {ex.Message}"
                    : $"Exception in {context}: {ex.Message}";
                FR2_LOG.LogError(message);
            }
        }
        
        internal static T SafeExecute<T>(this Func<T> func, T defaultValue = default(T), string context = null)
        {
            try
            {
                return func != null ? func.Invoke() : defaultValue;
            }
            catch (Exception ex)
            {
                string message = string.IsNullOrEmpty(context) 
                    ? $"Exception in SafeExecute: {ex.Message}"
                    : $"Exception in {context}: {ex.Message}";
                FR2_LOG.LogError(message);
                return defaultValue;
            }
        }

        internal static void SafeForEach<T>(this IEnumerable<T> collection, Action<T> action, string context = null)
        {
            if (collection == null || action == null) return;

            foreach (var item in collection)
            {
                try
                {
                    action(item);
                }
                catch (Exception ex)
                {
                    string message = string.IsNullOrEmpty(context) 
                        ? $"Exception in SafeForEach: {ex.Message}"
                        : $"Exception in {context} for item {item}: {ex.Message}";
                    FR2_LOG.LogError(message);
                }
            }
        }

    }
}