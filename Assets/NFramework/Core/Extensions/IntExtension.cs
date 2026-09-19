using UnityEngine;

namespace NFramework
{
    public static class IntExtension
    {
        public static int Clamp(this int value, int min, int max) => Mathf.Clamp(value, min, max);
        
        public static int Clamp01(this int value) => Mathf.Clamp(value, 0, 1);
    }
}