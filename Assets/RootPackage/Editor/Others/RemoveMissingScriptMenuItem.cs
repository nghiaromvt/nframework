using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    public static class RemoveMissingScriptMenuItem
    {
        [MenuItem("NFramework/Remove Missing Scripts/All Prefabs")]
        public static void RemoveMissingScriptsAllPrefabs()
        {
            string PATH = "Asset";

            EditorUtility.DisplayProgressBar("Modify Prefab", "Please wait...", 0);
            string[] ids = AssetDatabase.FindAssets("t:Prefab", new string[] { PATH });
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                int delCount = 0;
                RecursivelyModifyPrefabChilds(instance, ref delCount);

                if (delCount > 0)
                {
                    NLogger.Log($"Removed({delCount}) on {path}", prefab);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(instance, path, InteractionMode.AutomatedAction);
                }

                Object.DestroyImmediate(instance);
                EditorUtility.DisplayProgressBar("Modify Prefab", "Please wait...", i / (float)ids.Length);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("NFramework/Remove Missing Scripts/Selected Object")]
        public static void RemoveMissingScriptsInPrefabsOfSelectedObject()
        {
            var curObject = Selection.activeGameObject;
            if (curObject != null)
            {
                var delCount = 0;
                RecursivelyModifyPrefabChilds(curObject, ref delCount);
                NLogger.Log($"Removed({delCount}) on object {curObject.name}", curObject);
            }
        }

        private static void RecursivelyModifyPrefabChilds(GameObject obj, ref int delCount)
        {
            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    var _childObj = obj.transform.GetChild(i).gameObject;
                    RecursivelyModifyPrefabChilds(_childObj, ref delCount);
                }
            }

            int innerDelCount = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
            delCount += innerDelCount;
        }
    }
}