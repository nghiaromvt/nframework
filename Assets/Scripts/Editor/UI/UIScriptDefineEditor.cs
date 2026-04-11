using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NFramework.Editor
{
    public static class UIScriptDefineEditor
    {
        [MenuItem("NFramework/UI/Generate Script Define")]
        public static void GenerateScriptDefineStatic()
        {
            var config = NFrameworkConfigSO.GetConfig();
            
            if (string.IsNullOrEmpty(config.uiViewsFolderPath))
            {
                NLogger.LogError("No views folder path provided.");
                return;
            }

            if (string.IsNullOrEmpty(config.uiScriptDefineSavePath))
            {
                NLogger.LogError("No save path provided.");
                return;
            }
                
            var prefabs = FileHelper.LoadAssetsWithType<GameObject>("t:Prefab",
                $"Assets/{config.uiViewsFolderPath}");
            
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
            var nameSpace = NFrameworkConfigSO.GetConfig().scriptDefineNamespace;

            var (script, path) = GetScriptDefineInProject();
            if (script)
            {
                path = Path.GetDirectoryName(path).Replace(@"Assets\", "").Replace("Assets/", "");
                if (!string.Equals(config.uiScriptDefineSavePath, path))
                {
                    config.uiScriptDefineSavePath = path;
                    Debug.Log("Update uiScriptDefineSavePath");
                }
            }

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
            var fullPath = Path.Combine(Application.dataPath, config.uiScriptDefineSavePath, "UIDefine.cs");
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