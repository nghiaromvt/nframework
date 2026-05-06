using System;
using System.Collections.Generic;
using System.Linq;

namespace NFramework
{
    public static class EnumerableExtension
    {
        public static T GetRandom<T>(this IEnumerable<T> collection) => collection.ElementAt(UnityEngine.Random.Range(0, collection.Count()));
        
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();
        
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source) action(element);
            return source;
        }
    }
}