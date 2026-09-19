using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using NFramework.Sound;

namespace NFramework.Sound.Editor
{
    [Serializable]
    public class SoundGroupCreator
    {
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        [SerializeField, Required]
        private string _targetFolder;

        [SerializeField, Required, OnInspectorInit(nameof(OnKeyChanged)), OnValueChanged(nameof(OnKeyChanged))] 
        private string _assetName = "New Sound Group";
        [SerializeField, ReadOnly] private string _defineKeyConstName;
        
        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(HasError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;

        private bool HasError => !string.IsNullOrEmpty(_errorMessage);
        
        [SerializeField, Searchable] private List<SoundGroupSO.SoundEntry> _soundEntries = new();
        [Header("Script Define")] 
        [SerializeField] private bool _generateScriptDefine = true;
        
        private void OnKeyChanged()
        {
            _defineKeyConstName = _assetName.ToValidConstKey();

            if (string.IsNullOrWhiteSpace(_assetName))
            {
                _errorMessage = "\u26a0 Asset name must not be empty!";
                return;
            }

            var allGroupPaths = AssetDatabase.FindAssets("t:SoundGroupSO");
            foreach (var guid in allGroupPaths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, _assetName, StringComparison.OrdinalIgnoreCase))
                {
                    _errorMessage = $"\u26a0 Duplicate SoundGroup asset file name '{_assetName}' found at: {path}!";
                    return;
                }
            }

            _errorMessage = string.Empty;
        }

        [Button(ButtonSizes.Gigantic)]
        private void Create()
        {
            if (HasError || string.IsNullOrWhiteSpace(_assetName))
            {
                NLogger.LogError("Cannot create SoundGroup: validation failed or asset name is empty.");
                return;
            }

            var targetDirectory = string.IsNullOrEmpty(_targetFolder)
                ? "Assets"
                : Path.Combine("Assets", _targetFolder);

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var soundGroup = ScriptableObject.CreateInstance<SoundGroupSO>();
            soundGroup.soundEntries = _soundEntries;
            soundGroup.defineKeyConstName = _defineKeyConstName;

            var fullPath = Path.Combine(targetDirectory, _assetName);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(fullPath + ".asset");
            AssetDatabase.CreateAsset(soundGroup, uniqueFileName);
            AssetDatabase.SaveAssets();

            soundGroup.RefreshKey();
            soundGroup.ValidateKey();
            EditorUtility.SetDirty(soundGroup);
            AssetDatabase.SaveAssets();

            Selection.activeObject = soundGroup;

            if (_generateScriptDefine)
                SoundScriptDefineEditor.GenerateScriptDefine();
        }
    }
}