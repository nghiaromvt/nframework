using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace NFramework
{
    [Serializable]
    public class SoundData
    {
        public Action<AudioClip> onClipChanged;
        
        [OnInspectorInit(nameof(OnClipChanged)), OnValueChanged(nameof(OnClipChanged))] public AudioClip clip;
        [Range(0f, 1f)] public float volumeScale = 1f;

        public string PlaySfx(float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, EAudioOverlapType audioOverlapType = default, float fadeTime = 0f, Action onStop = null)
        {
            return SoundManager.PlaySfx(clip, volume * volumeScale, loop, pitch, ignorePause, audioOverlapType, fadeTime, onStop);
        }

        public void PlayBgm(float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignorePause = false, float fadeTime = 0f, Action onStop = null)
        {
            SoundManager.PlayBgm(clip, volume * volumeScale, loop, pitch, ignorePause, fadeTime, onStop);
        }

        private void OnClipChanged() => onClipChanged?.Invoke(clip);
    }
    
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundGroup", fileName = "New Sound Group")]
    public class SoundGroupSO : SerializedScriptableObject
    {
        [Serializable]
        public class SoundEntry
        {
            [ReadOnly] public string defineKeyConstName;
            [OnValueChanged(nameof(OnKeyChanged))] public string key;
            [HideLabel] public SoundData value;
            
            [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
            private string _errorMessage;
            private bool _showError;

            public SoundEntry()
            {
                value = new();
                value.onClipChanged = OnClipChanged;
            }

            private void OnClipChanged(AudioClip clip)
            {
                if (string.IsNullOrEmpty(key) && clip != null)
                {
                    key = clip.name;
                    OnKeyChanged();
                }
            }

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
        
        [FormerlySerializedAs("soundEntry")]
        [Space]
        [TabGroup("Audio Clip"), Searchable] public List<SoundEntry> soundEntries = new();

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