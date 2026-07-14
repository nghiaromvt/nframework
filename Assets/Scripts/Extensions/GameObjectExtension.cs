using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class GameObjectExtension
    {
        public static void SetLayerRecursively(this GameObject obj, LayerMask newLayer, List<GameObject> exclude = null, bool ignoreExcludeChild = false)
        {
            SetLayerRecursively(obj, newLayer.LayerMaskToLayer(), exclude, ignoreExcludeChild);
        }

        public static void SetLayerRecursively(this GameObject obj, int newLayer, List<GameObject> exclude = null, bool ignoreExcludeChild = false)
        {
            if (exclude == null || !exclude.Contains(obj))
                obj.layer = newLayer;
            else if (exclude.Contains(obj) && ignoreExcludeChild)
                return;
            
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                obj.transform.GetChild(i).gameObject.SetLayerRecursively(newLayer, exclude, ignoreExcludeChild);
            }
        }

        public static void SetActiveChildren(this GameObject obj, bool value)
        {
            for (int i = 0, length = obj.transform.childCount; i < length; ++i)
            {
                obj.transform.GetChild(i).gameObject.SetActive(value);
            }
        }
        
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out var component)
                ? component
                : gameObject.AddComponent<T>();
        }

        public static void SetActive(this IList<GameObject> objs, bool value)
        {
            for(int i = objs.Count - 1;  i >= 0; --i)
            {
                objs[i].SetActive(value);
            }
        }
        
        /// <summary>
        /// Checks whether the GameObject's layer is included in the provided layer mask.
        /// </summary>
        /// <param name="gameObject">The GameObject whose layer should be checked.</param>
        /// <param name="mask">The layer mask to test against.</param>
        /// <returns>True if the GameObject's layer is present in the mask, otherwise false.</returns>
        /// <example>
        /// <code>
        /// if (target.IsInLayerMask(attackableMask)) {
        ///     Attack(target);
        /// }
        /// </code>
        /// </example>
        public static bool IsInLayerMask(this GameObject gameObject, LayerMask mask) {
            return (mask.value & (1 << gameObject.layer)) != 0;
        }

        /// <summary>
        /// This method is used to hide the GameObject in the Hierarchy view.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void HideInHierarchy(this GameObject gameObject) {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }
    }
}