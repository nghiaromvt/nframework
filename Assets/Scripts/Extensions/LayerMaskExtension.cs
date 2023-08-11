using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class LayerMaskExtension
    {
        public static int GetRaycastValue(this LayerMask mask) => 1 << mask;

        /// <summary>
        /// Returns bool if layer index is within layermask
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer) => ((mask.value & (1 << layer)) > 0);

        /// <summary>
        /// Inverts a LayerMask.
        /// </summary>
        public static LayerMask Inverse(this LayerMask original) => ~original;

        /// <summary>
        /// Adds a number of layer names to an existing LayerMask.
        /// </summary>
        public static LayerMask AddToMask(this LayerMask original, params string[] layerNames)
        {
            return original | LayerMaskHelper.NamesToMask(layerNames);
        }

        /// <summary>
        /// Removes a number of layer names from an existing LayerMask.
        /// </summary>
        public static LayerMask RemoveFromMask(this LayerMask original, params string[] layerNames)
        {
            LayerMask invertedOriginal = ~original;
            return ~(invertedOriginal | LayerMaskHelper.NamesToMask(layerNames));
        }

        /// <summary>
        /// Returns a string array of layer names from a LayerMask.
        /// </summary>
        public static string[] MaskToNames(this LayerMask original)
        {
            var output = new List<string>();
            for (int i = 0; i < 32; ++i)
            {
                int shifted = 1 << i;
                if ((original & shifted) == shifted)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                        output.Add(layerName);
                }
            }
            return output.ToArray();
        }

        /// <summary>
        /// Returns an array of layer indexes from a LayerMask.
        /// </summary>
        public static int[] MaskToNumbers(this LayerMask original)
        {
            var output = new List<int>();
            for (int i = 0; i < 32; ++i)
            {
                int shifted = 1 << i;
                if ((original & shifted) == shifted)
                    output.Add(i);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Parses a LayerMask to a string.
        /// </summary>
        public static string MaskToString(this LayerMask original) => MaskToString(original, ", ");

        /// <summary>
        /// Parses a LayerMask to a string using the specified delimiter.
        /// </summary>
        public static string MaskToString(this LayerMask original, string delimiter)
        {
            return string.Join(delimiter, MaskToNames(original));
        }
    }
}