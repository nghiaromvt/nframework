using UnityEngine;

namespace NFramework
{
    public static class LayerMaskHelper
    {
        /// <summary>
        /// Creates a LayerMask from an array of layer names.
        /// </summary>
        public static LayerMask Create(params string[] layerNames) => NamesToMask(layerNames);

        /// <summary>
        /// Creates a LayerMask from a number of layer names.
        /// </summary>
        public static LayerMask NamesToMask(params string[] layerNames)
        {
            LayerMask ret = 0;
            foreach (var name in layerNames)
            {
                ret |= (1 << LayerMask.NameToLayer(name));
            }
            return ret;
        }

        /// <summary>
        /// Creates a LayerMask from an array of layer indexes.
        /// </summary>
        public static LayerMask Create(params int[] layerNumbers) => LayerNumbersToMask(layerNumbers);

        /// <summary>
        /// Creates a LayerMask from a number of layer indexes.
        /// </summary>
        public static LayerMask LayerNumbersToMask(params int[] layerNumbers)
        {
            LayerMask ret = (LayerMask)0;
            foreach (var layer in layerNumbers)
            {
                ret |= (1 << layer);
            }
            return ret;
        }
    }
}


