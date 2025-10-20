using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NFramework.Editor
{
    [Serializable]
    public class UIScriptDefineMenu
    {
        public static string SavePathPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("UIScriptDefineMenuSavePath");
        public static string NamespacePrefsKey => EditorHelper.GetUniqueProjectPrefsKey("UIScriptDefineMenuNamespace");

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField, OnValueChanged(nameof(OnSavePathChanged))]
        private string _savePath;

        [SerializeField, OnValueChanged(nameof(OnNameSpaceChanged))]
        private string _namespace = EditorPrefs.GetString(NamespacePrefsKey, EditorSettings.projectGenerationRootNamespace);

        private void OnSavePathChanged() => EditorPrefs.SetString(SavePathPrefsKey, _savePath);

        private void OnNameSpaceChanged() => EditorPrefs.SetString(NamespacePrefsKey, _namespace);
        
        public UIScriptDefineMenu()
        {
            var (script, path) = GetScriptDefineInProject();
            if (script)
                EditorPrefs.SetString(SavePathPrefsKey, Path.GetDirectoryName(path).Replace(@"Assets\", ""));
            
            _savePath = EditorPrefs.GetString(SavePathPrefsKey, "");
        }

        [Button(ButtonSizes.Gigantic)]
        private void LocateScriptDefine() => LocateScriptDefineStatic();

        [Button(ButtonSizes.Gigantic)]
        private void GenerateScriptDefine() => GenerateScriptDefineStatic();

        [MenuItem("NFramework/UI/Generate Script Define")]
        public static void GenerateScriptDefineStatic()
        {
            var prefabs = FileHelper.LoadAssetsWithType<GameObject>("t:Prefab");
            var uiLayerToViewsDict = new Dictionary<UILayer, List<UIView>>();
            prefabs.ForEach(x =>
            {
                if (x.TryGetComponent<UIView>(out var view))
                {
                    if (!uiLayerToViewsDict.ContainsKey(view.UILayer))
                        uiLayerToViewsDict[view.UILayer] = new List<UIView>();
                    
                    uiLayerToViewsDict[view.UILayer].Add(view);
                }
            });
            
            var stringBuilder = new StringBuilder();
            var nameSpace = EditorPrefs.GetString(NamespacePrefsKey, EditorSettings.projectGenerationRootNamespace);
            
            var (script, path) = GetScriptDefineInProject();
            if (script)
                EditorPrefs.SetString(SavePathPrefsKey, Path.GetDirectoryName(path).Replace(@"Assets\", ""));
            
            var savePath = EditorPrefs.GetString(SavePathPrefsKey, "");

            // Header
            stringBuilder.AppendLine("// This file is auto-generated.");
            stringBuilder.AppendLine("// Do not modify this file manually.\n");

            // Namespace open
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                stringBuilder.AppendLine($"namespace {nameSpace}");
                stringBuilder.AppendLine("{");
            }

            // UIDefine class open
            stringBuilder.AppendLine("\tpublic static class UIDefine");
            stringBuilder.AppendLine("\t{");

            foreach (var kv in uiLayerToViewsDict)
            {
                stringBuilder.AppendLine($"\t\t// {kv.Key}");
                foreach (var view in kv.Value)
                {
                    stringBuilder.AppendLine($"\t\tpublic static string {view.defineKeyConstName} = \"{view.key}\";");
                }
            }

            // Close UIDefine class
            stringBuilder.AppendLine("\t}");

            // Namespace close
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                stringBuilder.AppendLine("}");
            }

            // Write to file
            var fullPath = Path.Combine(Application.dataPath, savePath, "UIDefine.cs");
            File.WriteAllText(fullPath, stringBuilder.ToString());

            AssetDatabase.Refresh();
            NLogger.Log($"UIDefine.cs generated at: {fullPath}");
            LocateScriptDefineStatic();
        }

        [MenuItem("NFramework/UI/Locate Script Define")]
        public static void LocateScriptDefineStatic()
        {
            var (script, path) = GetScriptDefineInProject();
            
            if (script)
                EditorGUIUtility.PingObject(script);
            else
                NLogger.Log("Script not found.");
        }

        private static (Object, string) GetScriptDefineInProject()
        {
            var script = FileHelper.LoadFirstAssetWithName<Object>("UIDefine", "t:Script");
            
            if (!script)
                return (null, null);
            
            var path = AssetDatabase.GetAssetPath(script);
            return (script, path);
        }
    }
}