using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using NFramework.UI;

namespace NFramework.UI.Editor
{
    public static class UIScriptDefineEditor
    {
        [MenuItem("Tools/NFramework/UI/Generate Script Define")]
        public static void GenerateScriptDefine()
        {
            var config = UIConfigSO.GetConfig();
            
            if (string.IsNullOrEmpty(config.ScriptDefineSavePath))
            {
                NLogger.LogError("No save path provided.");
                return;
            }
                
            var guids = AssetDatabase.FindAssets("t:Prefab");
            var uiLayerToViewsDict = new Dictionary<UILayer, List<UIView>>();
            
            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var view = AssetDatabase.LoadAssetAtPath<UIView>(prefabPath);
                if (view != null)
                {
                    view.RefreshKey();
                    if (view.ValidateKey())
                    {
                        if (!uiLayerToViewsDict.ContainsKey(view.UILayer))
                            uiLayerToViewsDict[view.UILayer] = new List<UIView>();

                        uiLayerToViewsDict[view.UILayer].Add(view);
                    }
                    else
                    {
                        NLogger.LogError($"Skipping invalid UIView: {view.name} at {prefabPath}");
                    }
                }
            }

            var stringBuilder = new StringBuilder();
            var nameSpace = config.ScriptDefineNamespace;

            var (script, path) = GetScriptDefineInProject();
            if (script)
            {
                path = Path.GetDirectoryName(path).Replace(@"Assets\", "").Replace("Assets/", "");
                if (!string.Equals(config.ScriptDefineSavePath, path))
                {
                    config.ScriptDefineSavePath = path;
                    Debug.Log("Update scriptDefineSavePath");
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
                    stringBuilder.AppendLine($"\t\tpublic const string {view.DefineKeyConstName} = \"{view.Key}\";");
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
            var fullPath = Path.Combine(Application.dataPath, config.ScriptDefineSavePath, "UIDefine.cs");
            File.WriteAllText(fullPath, stringBuilder.ToString());

            AssetDatabase.Refresh();
            NLogger.Log($"UIDefine.cs generated at: {fullPath}");
            LocateScriptDefine();
        }

        [MenuItem("Tools/NFramework/UI/Locate Script Define")]
        public static void LocateScriptDefine()
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