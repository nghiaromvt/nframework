using UnityEngine;
using NFramework.UI;

namespace NFramework.UI.Editor
{
    public static class UIValidationHelper
    {
        public static string ValidateDuplicateKey(string viewKey, string constName, UIView excludeView = null)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var view = UnityEditor.AssetDatabase.LoadAssetAtPath<UIView>(path);
                if (view == null)
                {
                    continue;
                }

                if (excludeView != null && view == excludeView)
                {
                    continue;
                }

                if (view.Key == viewKey)
                {
                    return $"\u26a0 Duplicate key with other view: {view.name} at {path}!";
                }

                if (view.DefineKeyConstName == constName)
                {
                    return $"\u26a0 Duplicate define key const name with other view: {view.name} at {path}!";
                }
            }

            return null;
        }
    }
}
