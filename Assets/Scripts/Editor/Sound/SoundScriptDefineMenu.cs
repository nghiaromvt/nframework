using System;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace NFramework.Editor
{
    [Serializable]
    public class SoundScriptDefineMenu
    {
        public static string SavePathPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("SoundScriptDefineSavePath");
        public static string NamespacePrefsKey => EditorHelper.GetUniqueProjectPrefsKey("SoundScriptDefineNamespace");

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField, OnValueChanged(nameof(OnSavePathChanged))]
        private string _savePath = EditorPrefs.GetString(SavePathPrefsKey, "");

        [SerializeField, OnValueChanged(nameof(OnNameSpaceChanged))]
        private string _namespace = EditorPrefs.GetString(NamespacePrefsKey, EditorSettings.projectGenerationRootNamespace);

        private void OnSavePathChanged() => EditorPrefs.SetString(SavePathPrefsKey, _savePath);

        private void OnNameSpaceChanged() => EditorPrefs.SetString(NamespacePrefsKey, _namespace);

        [Button(ButtonSizes.Gigantic)]
        private void LocateScriptDefine() => LocateScriptDefineStatic();

        [Button(ButtonSizes.Gigantic)]
        private void GenerateScriptDefine() => GenerateScriptDefineStatic();

        [MenuItem("NFramework/Sound/Generate Script Define")]
        public static void GenerateScriptDefineStatic()
        {
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
            var stringBuilder = new StringBuilder();
            var nameSpace = EditorPrefs.GetString(NamespacePrefsKey, EditorSettings.projectGenerationRootNamespace);
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
            var fullPath = System.IO.Path.Combine(Application.dataPath, savePath, "SoundDefine.cs");
            System.IO.File.WriteAllText(fullPath, stringBuilder.ToString());

            AssetDatabase.Refresh();
            NLogger.Log($"SoundDefine.cs generated at: {fullPath}");
            LocateScriptDefineStatic();
        }

        [MenuItem("NFramework/Sound/Locate Script Define")]
        public static void LocateScriptDefineStatic()
        {
            var script = FileHelper.LoadFirstAssetWithName<Object>("SoundDefine", "SoundDefine");
            if (script)
                EditorGUIUtility.PingObject(script);
        }
    }
}