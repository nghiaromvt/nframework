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
        [SerializeField, Required] private string _loadKey;
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets"), SerializeField] private string _savePath = "";
        [TabGroup("Audio Clip"), Searchable] private List<SoundGroupSO.AudioClipData> _audioClipDatas = new();
        [TabGroup("Sound Info"), Searchable] private List<SoundGroupSO.SoundInfoData> _soundInfoDatas = new();
        [Header("Script Define")] 
        [SerializeField] private bool _generateScriptDefine = true;
        
        [Button(ButtonSizes.Gigantic)]
        private void Create()
        {
            var soundGroup = ScriptableObject.CreateInstance<SoundGroupSO>();
            soundGroup.audioClipDatas = _audioClipDatas;
            soundGroup.soundInfoDatas = _soundInfoDatas;
            soundGroup.loadKey = _loadKey;
            
            var fullPath = Path.Combine($"Assets/{_savePath}", _assetName);
            var uniqueFileName = AssetDatabase.GenerateUniqueAssetPath(fullPath + ".asset");
            AssetDatabase.CreateAsset(soundGroup, uniqueFileName);
            AssetDatabase.SaveAssets();
            
            Selection.activeObject = soundGroup;
        }
    }
}