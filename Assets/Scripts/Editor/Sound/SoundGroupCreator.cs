using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    [Serializable]
    public class SoundGroupCreator
    {
        [SerializeField, Required] private string _assetName = "New Sound Group";
        [SerializeField, ReadOnly] private string _defineKeyConstName;
        [SerializeField, OnInspectorInit(nameof(OnKeyChanged)), OnValueChanged(nameof(OnKeyChanged))] 
        private string _key;
        
        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;
        
        [SerializeField, Searchable] private List<SoundGroupSO.SoundEntry> _soundEntries = new();
        [Header("Script Define")] 
        [SerializeField] private bool _generateScriptDefine = true;
        
        private void OnKeyChanged()
        {
            _defineKeyConstName = _key.ToValidConstKey();
            
            var config = NFrameworkConfigSO.GetConfig();
                
            if (string.IsNullOrEmpty(config.soundGroupFolderPath))
            {
                _showError = true;
                _errorMessage = $"\u26a0 No sound group path provided!";
                return;
            }
                
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>(searchInFolder: $"Assets/{config.soundGroupFolderPath}");
            foreach (var soundGroup in soundGroups)
            {
                if (soundGroup.key == _key)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate key with other SoundGroup: {soundGroup.name}!";
                    return;
                }
            }
                
            _showError = false;
        }

        [Button(ButtonSizes.Gigantic)]
        private void Create()
        {
            var soundGroup = ScriptableObject.CreateInstance<SoundGroupSO>();
            soundGroup.soundEntries = _soundEntries;
            soundGroup.defineKeyConstName = _defineKeyConstName;
            soundGroup.key = _key;
            
            var config = NFrameworkConfigSO.GetConfig();
            if (config == null)
                return;
            
            var fullPath = Path.Combine($"Assets/{config.soundGroupFolderPath}", _assetName);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(fullPath + ".asset");
            AssetDatabase.CreateAsset(soundGroup, uniqueFileName);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = soundGroup;

            if (_generateScriptDefine)
                SoundScriptDefineEditor.GenerateScriptDefineStatic();
        }
    }
}