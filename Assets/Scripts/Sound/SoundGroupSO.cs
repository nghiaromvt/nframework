using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundGroup", fileName = "New Sound Group")]
    public class SoundGroupSO : SerializedScriptableObject
    {
        [Serializable]
        public class KeyValueData<T>
        {
            [ValidateInput(nameof(IsValidVariableName), 
                "Keys must start with a letter or underscore, and contain only letters, digits or underscores.")]
            [HideLabel, HorizontalGroup] public string key;
            [HideLabel, HorizontalGroup] public T value;
            
            private bool IsValidVariableName(string input)
            {
                // C# identifier rule: start with letter/_ ; then letters, digits or _
                return !string.IsNullOrEmpty(input)
                       && Regex.IsMatch(input, @"^[_a-zA-Z]\w*$");
            }
        }
        
        [Serializable]
        public class AudioClipData : KeyValueData<AudioClip> { }
        
        [Serializable]
        public class SoundInfoData : KeyValueData<SoundInfoSO> { }
        
        [TabGroup("Audio Clip"), Searchable] public List<AudioClipData> audioClipDatas = new();
        [TabGroup("Sound Info"), Searchable] public List<SoundInfoData> soundInfoDatas = new();
        [Header("Script Define")]
        public string loadKey;
        public string scriptNamespace = "";

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")] public string scriptSavePath = "";

#if UNITY_EDITOR
        [Button(ButtonSizes.Large)]
        public void GenerateScriptDefine()
        {
            GenerateScriptDefineWithoutRefresh();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
        [Button(ButtonSizes.Large), HorizontalGroup]
        public void LocateScriptDefine()
        {
            var path = Path.Combine($"Assets/{scriptSavePath}", $"{GetScriptDefineName()}.cs");
            var script = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (script)
                EditorGUIUtility.PingObject(script);
            else
                NLogger.Log("Not found script define");
        }
        
        [Button(ButtonSizes.Large), HorizontalGroup]
        public void Delete()
        {
            var path = AssetDatabase.GetAssetPath(GetInstanceID());
            if (EditorUtility.DisplayDialog("Delete this sound group and its script define?", path + "\n\nYou cannot undo this action", "Delete", "Cancel"))
            {
                var definePath = Path.Combine($"Assets/{scriptSavePath}", $"{GetScriptDefineName()}.cs");
                
                if (File.Exists(definePath))
                    File.Delete(definePath);
                
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
            }
        }
        
        public void GenerateScriptDefineWithoutRefresh()
        {
            var loadKeyBody = $"public const string LOAD_KEY = \"{loadKey}\";";

            var audioClipBuilder = new StringBuilder();
            if (!audioClipDatas.IsNullOrEmpty())
            {
                foreach (var kv in audioClipDatas)
                {
                    if (!string.IsNullOrEmpty(kv.key))
                        audioClipBuilder.AppendLine($"\t\t\tpublic const string {kv.key} = \"{kv.key}\";");
                }
            }

            var soundSOBuilder = new StringBuilder();
            if (!soundInfoDatas.IsNullOrEmpty())
            {
                foreach (var kv in soundInfoDatas)
                {
                    if (!string.IsNullOrEmpty(kv.key))
                        soundSOBuilder.AppendLine($"\t\t\tpublic const string {kv.key} = \"{kv.key}\";");
                }
            }

            var classBuilder = new StringBuilder();

            // Add auto-generated file comment
            classBuilder.AppendLine("// This file is auto-generated.");
            classBuilder.AppendLine("// Do not modify this file manually.\n");

            if (!string.IsNullOrEmpty(scriptNamespace))
            {
                classBuilder.AppendLine($"namespace {scriptNamespace}");
                classBuilder.AppendLine("{");
            }

            classBuilder.AppendLine($"\tpublic static class {GetScriptDefineName()}");
            classBuilder.AppendLine("\t{");
            classBuilder.AppendLine($"\t\t{loadKeyBody}\n");

            classBuilder.AppendLine("\t\tpublic static class AudioClip");
            classBuilder.AppendLine("\t\t{");
            classBuilder.Append(audioClipBuilder);
            classBuilder.AppendLine("\t\t}\n");

            classBuilder.AppendLine("\t\tpublic static class SoundSO");
            classBuilder.AppendLine("\t\t{");
            classBuilder.Append(soundSOBuilder);
            classBuilder.AppendLine("\t\t}");

            classBuilder.AppendLine("\t}");

            if (!string.IsNullOrEmpty(scriptNamespace))
            {
                classBuilder.AppendLine("}");
            }

            var filePath = Path.Combine(Application.dataPath, scriptSavePath, $"{GetScriptDefineName()}.cs");
            using (var sw = new StreamWriter(filePath))
            {
                sw.Write(classBuilder.ToString());
            }
        }
        
        private string GetScriptDefineName() => name.Replace(" ", "").Replace("SO", "Define");
#endif
    }
}