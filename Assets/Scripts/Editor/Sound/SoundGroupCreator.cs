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
        [SerializeField] private string _assetName = "New Sound Group";
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField] private string _savePath;
        [TabGroup("Audio Clip"), Searchable, ShowInInspector] private Dictionary<string, AudioClip> _audioClipDict = new();
        [TabGroup("Sound Info"), Searchable, ShowInInspector] private Dictionary<string, SoundInfoSO> _soundInfoDict = new();
        [Header("Script Define")] 
        [SerializeField] private bool _generateScriptDefine = true;
        [SerializeField] private string _loadKey;
        [SerializeField] private string _scriptNamespace = "";
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField] public string _scriptSavePath = "";
        
        [Button(ButtonSizes.Gigantic)]
        private void Create()
        {
            var soundGroup = ScriptableObject.CreateInstance<SoundGroupSO>();
            soundGroup.audioClipDict = _audioClipDict;
            soundGroup.soundInfoDict = _soundInfoDict;
            soundGroup.loadKey = _loadKey;
            soundGroup.scriptNamespace = _scriptNamespace;
            soundGroup.scriptSavePath = _scriptSavePath;
            
            var fullPath = Path.Combine($"Assets/{_savePath}", _assetName);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(fullPath + ".asset");
            AssetDatabase.CreateAsset(soundGroup, uniqueFileName);
            AssetDatabase.SaveAssets();
            
            if (_generateScriptDefine)
                soundGroup.GenerateScriptDefine();
            
            Selection.activeObject = soundGroup;
        }
    }
}