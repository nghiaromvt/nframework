using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/SoundContainerSO")]
    public class SoundContainerSO : SerializedScriptableObject
    {
        [Searchable] public Dictionary<string, AudioClip> audioClipDict = new();
        [Searchable] public Dictionary<string, SoundSO> soundSODict = new();

        [Space, Header("Script define")] 
        [Required] public string loadKey;
        [Required] public string scriptNamespace = "";
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")] public string scriptSavePath;
        
#if UNITY_EDITOR
        [Button(ButtonSizes.Large)]
        private void CreateScriptDefine()
        {
            CreateScriptWithoutRefresh();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
        
        public void CreateScriptWithoutRefresh()
        {
            var audioClipBody = "";
            var soundSOBody = "";
            var className = name.Replace(" ", "").Replace("ContainerSO", "Define");
            var loadKeyBody = $"public const string LOAD_KEY = \"{loadKey}\";";
            
            if (!audioClipDict.IsNullOrEmpty())
            {
                audioClipBody = audioClipDict.Aggregate(audioClipBody, (s, kv) =>
                {
                    if (string.IsNullOrEmpty(kv.Key))
                        return s;
                    
                    return s + $"\t\t\tpublic const string {kv.Key} = \"{kv.Key}\";\n";
                }); 
            }
            
            if (!soundSODict.IsNullOrEmpty())
            {
                soundSOBody = soundSODict.Aggregate(soundSOBody, (s, kv) =>
                {
                    if (string.IsNullOrEmpty(kv.Key))
                        return s;
                    
                    return s + $"\t\t\tpublic const string {kv.Key} = \"{kv.Key}\";\n";
                }); 
            }
            
            const string template = "namespace $[namespace]\n{\n\tpublic static class $[className]\n\t{\n\t\t$[loadKeyBody]\n\n\t\tpublic static class AudioClip\n\t\t{\n$[audioClipBody]\t\t}\n\n\t\tpublic static class SoundSO\n\t\t{\n$[soundSOBody]\t\t}\n\t}\n}";
            var arguments = new Dictionary<string, string>
            {
                { "namespace", scriptNamespace},
                { "className", className },
                { "loadKeyBody", loadKeyBody },
                { "audioClipBody", audioClipBody },
                { "soundSOBody", soundSOBody },
            };
            
            var soundDefine = arguments.Aggregate(template, (current, argument) => current.Replace($"$[{argument.Key}]", argument.Value));
            using (var sw = new StreamWriter(Path.Combine(Application.dataPath, scriptSavePath, $"{className}.cs")))
            {
                sw.Write(soundDefine);
            }
        }
#endif
    }
}