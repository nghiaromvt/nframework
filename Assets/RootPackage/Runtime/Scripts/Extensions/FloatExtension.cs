using UnityEngine;

namespace NFramework
{
    public static class FloatExtension
    {
        public static float Clamp(this float value, float min, float max) => Mathf.Clamp(value, min, max);
        
        public static float Clamp01(this float value) => Mathf.Clamp01(value);
        
        /// <summary>
        /// Snap to grid of "round" size
        /// </summary>
        public static float Snap(this float val, float round) => round * Mathf.Round(val / round);
		
        public static float Round(this float val) => Mathf.Round(val);
        
        public static int RoundToInt(this float value) => Mathf.RoundToInt(value);
        
        public static float Ceil(this float value) => Mathf.CeilToInt(value);
        
        public static int CeilToInt(this float value) => Mathf.CeilToInt(value);
        
        public static float Floor(this float value) => Mathf.FloorToInt(value);
        
        public static int FloorToInt(this float value) => Mathf.FloorToInt(value);
        
        /// <summary>
        /// Shortcut for Mathf.Approximately
        /// </summary>
        public static bool Approximately(this float value, float compare) => Mathf.Approximately(value, compare);
        
        /// <summary>
        /// Maps a value from some range to the 0 to 1 range
        /// </summary>
        public static float RemapTo01(this float value, float min, float max) => (value - min) * 1f / (max - min);
        
        /// <summary>
        /// Maps a value from one range to another
        /// </summary>
        public static float Remap(this float value, float leftMin, float leftMax, float rightMin, float rightMax) =>
            rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
    }
}