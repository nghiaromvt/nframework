using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editors
{
    /// <summary>
    /// Tool to replace all selected objects in the scene by a prefab. Much faster than doing it one by one
    /// </summary>
    public class ReplacePrefab : ScriptableWizard
    {
        public GameObject newPrefab;

        [MenuItem("NFramework/Replace Prefab", priority = 100)]
        private static void ScriptableWizardMenu() => DisplayWizard<ReplacePrefab>("Replace Prefab", "Replace");

        private void OnWizardCreate() => DoReplacePrefab();

        private void OnWizardUpdate() => helpString = "Use this tool to replace all selected objects in the scene by a prefab.";

        private void DoReplacePrefab()
        {
            if (newPrefab != null)
            {
                List<GameObject> newObjs = new List<GameObject>();

                foreach (Transform transform in Selection.transforms)
                {
                    GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(newPrefab);
                    Undo.RegisterCreatedObjectUndo(newObject, "Created prefab");

                    newObject.transform.position = transform.position;
                    newObject.transform.rotation = transform.rotation;
                    newObject.transform.localScale = transform.localScale;
                    newObject.transform.parent = transform.parent;
                    newObjs.Add(newObject);

                    Undo.DestroyObjectImmediate(transform.gameObject);
                }

                Selection.objects = newObjs.ToArray();
                Logger.Log("Replace prefab completed!", this);
            }
        }
    }
}
