using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class IListExtension
    {
        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temporary = list[i];
            list[i] = list[j];
            list[j] = temporary;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list.Swap(i, Random.Range(i, list.Count));
            }
        }

        /// <summary>
        /// Return a random item from the list.
        /// Sampling with replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RandomItem<T>(this IList<T> list)
        {
            if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot select a random item from an empty list");
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Removes a random item from the list, returning that item.
        /// Sampling without replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T RemoveRandom<T>(this IList<T> list)
        {
            if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot remove a random item from an empty list");
            int index = UnityEngine.Random.Range(0, list.Count);
            T item = list[index];
            list.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Check null of list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this IList<T> list) => list == null;

        /// <summary>
        /// Check empty of list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsEmpty<T>(this IList<T> list) => list.Count <= 0;

        /// <summary>
        /// Check element is out of list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsIndexOutOfList<T>(this IList<T> list, int index)
        {
            return (index < 0) || (index >= list.Count);
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list.Count <= 0;
    }
}