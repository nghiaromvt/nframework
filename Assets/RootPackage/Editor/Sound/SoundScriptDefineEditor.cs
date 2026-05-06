using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NFramework.Editor
{
    [Serializable]
    public class SoundScriptDefineEditor
    {
        [MenuItem("NFramework/Sound/Generate Script Define")]
        public static void GenerateScriptDefine()
        {
            var config = NFrameworkConfigSO.GetConfig();
            
            if (string.IsNullOrEmpty(config.soundGroupFolderPath))
            {
                NLogger.LogError("No sound group folder path provided.");
                return;
            }

            if (string.IsNullOrEmpty(config.soundScriptDefineSavePath))
            {
                NLogger.LogError("No save path provided.");
                return;
            }
            
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>(searchInFolder: $"Assets/{config.soundGroupFolderPath}");
            var stringBuilder = new StringBuilder();
            var nameSpace = config.scriptDefineNamespace;
            
            var (script, path) = GetScriptDefineInProject();
            if (script)
            {
                path = Path.GetDirectoryName(path).Replace(@"Assets\", "").Replace("Assets/", "");
                if (!string.Equals(config.soundScriptDefineSavePath, path))
                {
                    config.soundScriptDefineSavePath = path;
                    Debug.Log("Update soundScriptDefineSavePath");
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

            // SoundDefine class open
            stringBuilder.AppendLine("\tpublic static class SoundDefine");
            stringBuilder.AppendLine("\t{");

            // SoundGroupKey class
            stringBuilder.AppendLine("\t\tpublic static class SoundGroupKey");
            stringBuilder.AppendLine("\t\t{");
            foreach (var group in soundGroups)
            {
                if (group.defineKeyConstName.IsNullOrEmpty()) continue;
                stringBuilder.AppendLine($"\t\t\tpublic const string {group.defineKeyConstName} = \"{group.key}\";");
            }

            stringBuilder.AppendLine("\t\t}");
            stringBuilder.AppendLine();

            // SoundEntryKey class
            stringBuilder.AppendLine("\t\tpublic static class SoundEntryKey");
            stringBuilder.AppendLine("\t\t{");
            foreach (var group in soundGroups)
            {
                foreach (var soundEntry in group.soundEntries)
                {
                    if (soundEntry.defineKeyConstName.IsNullOrEmpty()) continue;
                    stringBuilder.AppendLine($"\t\t\tpublic const string {soundEntry.defineKeyConstName} = \"{soundEntry.key}\";");
                }
            }

            stringBuilder.AppendLine("\t\t}");
            stringBuilder.AppendLine();

            // Close SoundDefine class
            stringBuilder.AppendLine("\t}");

            // Namespace close
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                stringBuilder.AppendLine("}");
            }

            // Write to file
            var fullPath = Path.Combine(Application.dataPath, config.soundScriptDefineSavePath, "SoundDefine.cs");
            File.WriteAllText(fullPath, stringBuilder.ToString());

            AssetDatabase.Refresh();
            NLogger.Log($"SoundDefine.cs generated at: {fullPath}");
            LocateScriptDefine();
        }

        [MenuItem("NFramework/Sound/Locate Script Define")]
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
            var script = FileHelper.LoadFirstAssetWithName<Object>("SoundDefine", "t:Script");
            
            if (!script)
                return (null, null);
            
            var path = AssetDatabase.GetAssetPath(script);
            return (script, path);
        }
    }
}