using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
#endif
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundGroup", fileName = "New Sound Group")]
    public class SoundGroupSO : SerializedScriptableObject
    {
        [Serializable]
        public class KeyValueData<T>
        {
            [ReadOnly] public string defineKeyConstName;
            [HideLabel, HorizontalGroup, OnInspectorInit(nameof(OnKeyChanged)) ,OnValueChanged(nameof(OnKeyChanged))] public string key;
            [HideLabel, HorizontalGroup] public T value;
            
            [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
            private string _errorMessage;
            private bool _showError;
            
            private void OnKeyChanged()
            {
                defineKeyConstName = key.ToValidConstKey();
                
#if UNITY_EDITOR
                if (string.IsNullOrEmpty(key))
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Key must not be empty!";
                    return;
                }
                
                var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
                foreach (var soundGroup in soundGroups)
                {
                    foreach (var audioClipData in soundGroup.audioClipDatas)
                    {
                        if ((object)audioClipData == this)
                            continue;

                        if (audioClipData.key == key)
                        {
                            _showError = true;
                            _errorMessage = $"\u26a0 Duplicate key with other audioClipData from SoundGroup: {soundGroup.name}!";
                            return;
                        }
                    }
                    
                    foreach (var soundInfoData in soundGroup.soundInfoDatas)
                    {
                        if ((object)soundInfoData == this)
                            continue;

                        if (soundInfoData.key == key)
                        {
                            _showError = true;
                            _errorMessage = $"\u26a0 Duplicate key with other soundInfoData from SoundGroup: {soundGroup.name}!";
                            return;
                        }
                    }
                }
                
                _showError = false;
#endif
            }
        }
        
        [Serializable]
        public class AudioClipData : KeyValueData<AudioClip> { }
        
        [Serializable]
        public class SoundInfoData : KeyValueData<SoundInfoSO> { }
        
        [ReadOnly] public string defineKeyConstName;
        [OnInspectorInit(nameof(OnKeyChanged)) ,OnValueChanged(nameof(OnKeyChanged))] public string key;
            
        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;
        
        [Space]
        [TabGroup("Audio Clip"), Searchable] public List<AudioClipData> audioClipDatas = new();
        [TabGroup("Sound Info"), Searchable] public List<SoundInfoData> soundInfoDatas = new();

        private void OnKeyChanged()
        {
            defineKeyConstName = key.ToValidConstKey();
            
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = $"\u26a0 Key must not be empty!";
                return;
            }
                
            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
            foreach (var soundGroup in soundGroups)
            {
                if ((object)soundGroup == this)
                    continue;

                if (soundGroup.key == key)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate key with other SoundGroup: {soundGroup.name}!";
                    return;
                }
            }
                
            _showError = false;
#endif
        }
    }
}