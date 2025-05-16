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
        public static string SavePathPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("SoundGroupCreatorSavePath");
        
        [SerializeField, Required] private string _assetName = "New Sound Group";
        [SerializeField, ReadOnly] private string _defineKeyConstName;
        [SerializeField, OnInspectorInit(nameof(OnKeyChanged)), OnValueChanged(nameof(OnKeyChanged))] 
        private string _key;
        
        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;
        
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField, OnValueChanged(nameof(OnSavePathChanged))] 
        private string _savePath = EditorPrefs.GetString(EditorHelper.GetUniqueProjectPrefsKey(SavePathPrefsKey), "");

        [SerializeField, Searchable] private List<SoundGroupSO.SoundEntry> _soundEntries = new();
        [Header("Script Define")] 
        [SerializeField] private bool _generateScriptDefine = true;
        
        private void OnSavePathChanged() => EditorPrefs.SetString(SavePathPrefsKey, _savePath);

        private void OnKeyChanged()
        {
            _defineKeyConstName = _key.ToValidConstKey();
            
            if (string.IsNullOrEmpty(_key))
            {
                _showError = true;
                _errorMessage = $"\u26a0 Key must not be empty!";
                return;
            }
                
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
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
            
            var fullPath = Path.Combine($"Assets/{_savePath}", _assetName);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(fullPath + ".asset");
            AssetDatabase.CreateAsset(soundGroup, uniqueFileName);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = soundGroup;

            if (_generateScriptDefine)
                SoundScriptDefineMenu.GenerateScriptDefineStatic();
        }
    }
}