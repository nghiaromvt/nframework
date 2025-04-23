using System;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    [Serializable]
    public class SoundScriptDefineMenu
    {
        public const string SAVE_PATH_PREFS_KEY = "SoundScriptDefineSavePath";
        public const string NAME_SPACE_PREFS_KEY = "SoundScriptDefineNameSpace";

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField, OnValueChanged(nameof(OnSavePathChanged))]
        private string _savePath = EditorPrefs.GetString(EditorHelper.GetUniqueProjectPrefsKey(SAVE_PATH_PREFS_KEY), "");

        [SerializeField, OnValueChanged(nameof(OnNameSpaceChanged))]
        private string _nameSpace = EditorPrefs.GetString(EditorHelper.GetUniqueProjectPrefsKey(NAME_SPACE_PREFS_KEY),
            EditorSettings.projectGenerationRootNamespace);

        private void OnSavePathChanged()
        {
            var key = EditorHelper.GetUniqueProjectPrefsKey(SAVE_PATH_PREFS_KEY);
            EditorPrefs.SetString(key, _savePath);
        }

        private void OnNameSpaceChanged()
        {
            var key = EditorHelper.GetUniqueProjectPrefsKey(NAME_SPACE_PREFS_KEY);
            EditorPrefs.SetString(key, _nameSpace);
        }

        [Button(ButtonSizes.Gigantic)]
        private void LocateScriptDefine() => LocateScriptDefineStatic();

        [Button(ButtonSizes.Gigantic)]
        private void GenerateScriptDefine()
        {
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
            var stringBuilder = new StringBuilder();

            // Header
            stringBuilder.AppendLine("// This file is auto-generated.");
            stringBuilder.AppendLine("// Do not modify this file manually.\n");

            // Namespace open
            if (!string.IsNullOrWhiteSpace(_nameSpace))
            {
                stringBuilder.AppendLine($"namespace {_nameSpace}");
                stringBuilder.AppendLine("{");
            }

            // SoundDefine class open
            stringBuilder.AppendLine("\tpublic static class SoundDefine");
            stringBuilder.AppendLine("\t{");

            // LoadKey class
            stringBuilder.AppendLine("\t\tpublic static class GroupKey");
            stringBuilder.AppendLine("\t\t{");
            foreach (var group in soundGroups)
            {
                var fieldName = ToValidConstKey(group.loadKey);
                stringBuilder.AppendLine($"\t\t\tpublic const string {fieldName} = \"{group.loadKey}\";");
            }

            stringBuilder.AppendLine("\t\t}");
            stringBuilder.AppendLine();

            // AudioClipKey class
            stringBuilder.AppendLine("\t\tpublic static class AudioClipKey");
            stringBuilder.AppendLine("\t\t{");
            foreach (var group in soundGroups)
            {
                foreach (var clipData in group.audioClipDatas)
                {
                    stringBuilder.AppendLine($"\t\t\tpublic const string {ToValidConstKey(clipData.key)} = \"{clipData.key}\";");
                }
            }

            stringBuilder.AppendLine("\t\t}");
            stringBuilder.AppendLine();

            // SoundInfoKey class
            stringBuilder.AppendLine("\t\tpublic static class SoundInfoKey");
            stringBuilder.AppendLine("\t\t{");
            foreach (var group in soundGroups)
            {
                foreach (var infoData in group.soundInfoDatas)
                {
                    stringBuilder.AppendLine($"\t\t\tpublic const string {ToValidConstKey(infoData.key)} = \"{infoData.key}\";");
                }
            }

            stringBuilder.AppendLine("\t\t}");

            // Close SoundDefine class
            stringBuilder.AppendLine("\t}");

            // Namespace close
            if (!string.IsNullOrWhiteSpace(_nameSpace))
            {
                stringBuilder.AppendLine("}");
            }

            // Write to file
            var fullPath = System.IO.Path.Combine(Application.dataPath, _savePath, "SoundDefine.cs");
            System.IO.File.WriteAllText(fullPath, stringBuilder.ToString());

            AssetDatabase.Refresh();
            NLogger.Log($"SoundDefine.cs generated at: {_savePath}");
        }

        [MenuItem("NFramework/Sound/Update Script Define")]
        private static void UpdateScriptDefine()
        {
        }

        [MenuItem("NFramework/Sound/Locate Script Define")]
        private static void LocateScriptDefineStatic()
        {
            var script = FileHelper.LoadFirstAssetWithName<MonoScript>("SoundDefine", "SoundDefine");
            if (!script)
                EditorGUIUtility.PingObject(script);
        }
        
        private static string ToValidConstKey(string rawKey)
        {
            if (string.IsNullOrWhiteSpace(rawKey))
                return "_";

            var builder = new StringBuilder();

            foreach (char c in rawKey)
            {
                if (char.IsWhiteSpace(c))
                {
                    builder.Append('_');
                }
                else if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
            }

            // Ensure the first character is a letter or underscore
            if (builder.Length == 0 || (!char.IsLetter(builder[0]) && builder[0] != '_'))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString().ToUpperInvariant();
        }
    }
}