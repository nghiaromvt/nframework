using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    [CreateAssetMenu(menuName = "NFramework/Sound/SoundGroup", fileName = "New Sound Group")]
    public class SoundGroupSO : SerializedScriptableObject
    {
        [ReadOnly] public string defineKeyConstName;
        [ReadOnly] public string key;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)]
        private string _errorMessage;

        private bool _showError;

        [Space] [Searchable]
        public List<SoundEntry> soundEntries = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshKey();
            ValidateKey();
        }

        private void RefreshKey()
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            key = string.IsNullOrEmpty(assetPath)
                ? name
                : System.IO.Path.GetFileNameWithoutExtension(assetPath);

            defineKeyConstName = key.ToValidConstKey();
        }

        private void ValidateKey()
        {
            defineKeyConstName = key.ToValidConstKey();

            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = "\u26a0 Key must not be empty!";
                return;
            }

            var config = NFrameworkConfigSO.GetConfig();

            if (string.IsNullOrEmpty(config.soundGroupFolderPath))
            {
                _showError = true;
                _errorMessage = "\u26a0 No sound group path provided!";
                return;
            }

            var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>(
                searchInFolder: $"Assets/{config.soundGroupFolderPath}"
            );

            foreach (var soundGroup in soundGroups)
            {
                if (ReferenceEquals(soundGroup, this))
                    continue;

                if (soundGroup.defineKeyConstName == defineKeyConstName)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate define key const with other SoundGroup: {soundGroup.name}!";
                    return;
                }
                
                if (soundGroup.key == key)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate key with other SoundGroup: {soundGroup.name}!";
                    return;
                }
            }

            _showError = false;
            _errorMessage = string.Empty;
        }
#endif

        #region SoundEntry

        [Serializable]
        public class SoundEntry
        {
            [ReadOnly] public string defineKeyConstName;

            [OnInspectorInit(nameof(OnClipChanged)), OnValueChanged(nameof(OnKeyChanged))]
            public string key;

            [OnInspectorInit(nameof(OnClipChanged)), OnValueChanged(nameof(OnClipChanged))]
            public AudioClip clip;

            public SoundPlaySettings playSettings = new();

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

                        if (soundEntry.defineKeyConstName == defineKeyConstName)
                        {
                            _showError = true;
                            _errorMessage = $"\u26a0 Duplicate define key const with other SoundEntry from SoundGroup: {soundGroup.name}!";
                            return;
                        }

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
        
        #endregion
    }
}