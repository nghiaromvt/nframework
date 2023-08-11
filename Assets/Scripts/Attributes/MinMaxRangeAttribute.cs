using System;
using UnityEngine;

namespace NFramework
{
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        public MinMaxRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public readonly float min;
        public readonly float max;
    }

    [Serializable]
    public struct RangedFloat
    {
        public float min;
        public float max;

        public RangedFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [Serializable]
    public struct RangedInt
    {
        public int min;
        public int max;

        public RangedInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public static class RangedExtensions
    {
        public static float LerpFromRange(this RangedFloat ranged, float t)
        {
            return Mathf.Lerp(ranged.min, ranged.max, t);
        }

        public static float LerpFromRangeUnclamped(this RangedFloat ranged, float t)
        {
            return Mathf.LerpUnclamped(ranged.min, ranged.max, t);
        }

        public static float LerpFromRange(this RangedInt ranged, float t)
        {
            return Mathf.Lerp(ranged.min, ranged.max, t);
        }

        public static float LerpFromRangeUnclamped(this RangedInt ranged, float t)
        {
            return Mathf.LerpUnclamped(ranged.min, ranged.max, t);
        }
    }
}
