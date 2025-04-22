using System;
using Sirenix.OdinInspector;
using UnityEditor;

namespace NFramework.Editor
{
    [Serializable]
    public class GenerateAllScriptDefinesMenu
    {
        [MenuItem("NFramework/Sound/Generate All Script Defines")]
        private static void GenerateAllScriptDefinesStatic()
        {
            var soundGroupGuids = AssetDatabase.FindAssets($"t:{nameof(SoundGroupSO)}");
            soundGroupGuids.ForEach(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var soundGroup = AssetDatabase.LoadAssetAtPath<SoundGroupSO>(path);
                soundGroup.GenerateScriptDefineWithoutRefresh();
            });
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            NLogger.Log("All Sound Defines have been generated.");
        }
        
        [Button(ButtonSizes.Gigantic)]
        private void GenerateAllScriptDefines() => GenerateAllScriptDefinesStatic();
    }
}