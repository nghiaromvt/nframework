using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundGroup", fileName = "New Sound Group")]
    public class SoundGroupSO : SerializedScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            [ReadOnly] public string defineKeyConstName;
            [OnInspectorInit(nameof(OnClipChanged)), OnValueChanged(nameof(OnKeyChanged))] public string key;
            [OnInspectorInit(nameof(OnClipChanged)), OnValueChanged(nameof(OnClipChanged))] public AudioClip clip;
            [HideLabel] public SoundPlaySettings playSettings = new();
            
            [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
            private string _errorMessage;
            private bool _showError;
            
            private void OnClipChanged()
            {
                if (string.IsNullOrEmpty(key) && clip != null)
                {
                    key = clip.name;
                    OnKeyChanged();
                }
            }

            public void OnKeyChanged()
            {
#if UNITY_EDITOR
                defineKeyConstName = key.ToValidConstKey();
                
                if (string.IsNullOrEmpty(key))
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Key must not be empty!";
                    return;
                }
                
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
                    foreach (var soundEntry in soundGroup.soundEntries)
                    {
                        if (soundEntry == this)
                            continue;

                        if (soundEntry.key == key)
                        {
                            _showError = true;
                            _errorMessage = $"\u26a0 Duplicate key with other SoundEntry from SoundGroup: {soundGroup.name}!";
                            return;
                        }
                    }
                }
                
                _showError = false;
#endif
            }
        }
        
        [ReadOnly] public string defineKeyConstName;
        [OnInspectorInit(nameof(OnKeyChanged)) ,OnValueChanged(nameof(OnKeyChanged))] public string key;
            
        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;
        
        [Space]
        [TabGroup("Audio Clip"), Searchable] public List<SoundEntry> soundEntries = new();

        private void OnKeyChanged()
        {
#if UNITY_EDITOR
            defineKeyConstName = key.ToValidConstKey();
            
            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = $"\u26a0 Key must not be empty!";
                return;
            }
            
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