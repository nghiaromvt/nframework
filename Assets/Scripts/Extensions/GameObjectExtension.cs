using UnityEngine;

namespace NFramework
{
    public static class GameObjectExtension
    {
        public static void SetLayerRecursively(this GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                obj.transform.GetChild(i).gameObject.SetLayerRecursively(newLayer);
            }
        }

        public static void SetActiveChilds(this GameObject obj, bool value)
        {
            for (int i = 0, length = obj.transform.childCount; i < length; ++i)
            {
                obj.transform.GetChild(i).gameObject.SetActive(value);
            }
        }

        public static bool IsPrefab(this GameObject obj) => obj.scene.rootCount == 0;
    }
}