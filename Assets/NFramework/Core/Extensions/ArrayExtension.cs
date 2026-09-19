using UnityEngine;

namespace NFramework
{
    public static class ArrayExtension
    {
        public static void Swap<T>(this T[] array, int i, int j)
        {
            (array[i], array[j]) = (array[j], array[i]);
        }

        public static void Shuffle<T>(this T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array.Swap(i, Random.Range(i, array.Length));
            }
        }

        public static T RandomItem<T>(this T[] array)
        {
            if (array.Length == 0) throw new System.IndexOutOfRangeException("Cannot select a random item from an empty length");
            return array[Random.Range(0, array.Length)];
        }

        public static bool IsIndexOutOfRange<T>(this T[] array, int index)
        {
            return (index < 0) || (index >= array.Length);
        }
        
        public static T GetRandom<T>(this T[] collection) => collection[UnityEngine.Random.Range(0, collection.Length)];
        
        public static bool IsNullOrEmpty<T>(this T[] collection) => collection == null || collection.Length == 0;
    }
}