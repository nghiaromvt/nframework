using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class GameObjectExtension
    {
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
    }
}