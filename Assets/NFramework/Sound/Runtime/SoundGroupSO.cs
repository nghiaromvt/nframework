using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework.Sound
{
    public class SoundGroupSO : ScriptableObject
    {
        [ReadOnly] public string defineKeyConstName;
        [ReadOnly] public string key;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(HasError)), GUIColor(1, 0.3f, 0.3f)]
        private string _errorMessage;

        private bool HasError => !string.IsNullOrEmpty(_errorMessage);

        [Space] [Searchable]
        public List<SoundEntry> soundEntries = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            RefreshKey();
            ValidateKey();
        }

        public void RefreshKey()
        {
            defineKeyConstName = name.ToValidConstKey();

            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                key = name;
                return;
            }

            var inResources = TryGetResourcesRelativePath(assetPath, out var resourcesPath);
            var inAddressables = TryGetAddressablesAddress(assetPath, out var address);

            if (inResources && inAddressables)
            {
                key = string.Empty;
                return;
            }

            if (inResources)
            {
                key = resourcesPath;
                return;
            }

            if (inAddressables)
            {
                key = address;
                return;
            }

            key = string.Empty;
        }

        public bool ValidateKey()
        {
            defineKeyConstName = name.ToValidConstKey();

            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
            {
                _errorMessage = string.Empty;
                return true;
            }

            var inResources = TryGetResourcesRelativePath(assetPath, out var resourcesPath);
            var inAddressables = TryGetAddressablesAddress(assetPath, out var address);

            if (inResources && inAddressables)
            {
                _errorMessage = "\u26a0 Asset cannot be both in Resources/ and marked as Addressable!";
                return false;
            }

            if (!inResources && !inAddressables)
            {
                _errorMessage = "\u26a0 Asset must be either inside a Resources/ folder or marked as Addressable!";
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                _errorMessage = "\u26a0 Key must not be empty!";
                return false;
            }

            var allGroupPaths = UnityEditor.AssetDatabase.FindAssets("t:SoundGroupSO");
            foreach (var guid in allGroupPaths)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                if (path == assetPath)
                    continue;

                var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, name, StringComparison.OrdinalIgnoreCase))
                {
                    _errorMessage = $"\u26a0 Duplicate SoundGroup asset file name '{name}' found at: {path}!";
                    return false;
                }
            }

            _errorMessage = string.Empty;
            return true;
        }

        private static bool TryGetResourcesRelativePath(string assetPath, out string relativePath)
        {
            const string marker = "Resources/";
            var index = assetPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var sub = assetPath.Substring(index + marker.Length);
                relativePath = System.IO.Path.ChangeExtension(sub, null);
                return true;
            }

            relativePath = null;
            return false;
        }

        private static bool TryGetAddressablesAddress(string assetPath, out string address)
        {
            var defaultObjectType = Type.GetType("UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject, Unity.Addressables.Editor");
            if (defaultObjectType != null)
            {
                var settingsExistsProp = defaultObjectType.GetProperty("SettingsExists", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var settingsProp = defaultObjectType.GetProperty("Settings", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                if (settingsExistsProp != null && (bool)settingsExistsProp.GetValue(null) && settingsProp != null)
                {
                    var settings = settingsProp.GetValue(null);
                    if (settings != null)
                    {
                        var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                        var findEntryMethod = settings.GetType().GetMethod("FindAssetEntry", new[] { typeof(string), typeof(bool) });
                        if (findEntryMethod != null)
                        {
                            var entry = findEntryMethod.Invoke(settings, new object[] { guid, true });
                            if (entry != null)
                            {
                                var addressProp = entry.GetType().GetProperty("address");
                                if (addressProp != null)
                                {
                                    address = addressProp.GetValue(entry) as string;
                                    return !string.IsNullOrEmpty(address);
                                }
                            }
                        }
                    }
                }
            }

            address = null;
            return false;
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

            [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(HasError)), GUIColor(1, 0.3f, 0.3f)]
            private string _errorMessage;

            private bool HasError => !string.IsNullOrEmpty(_errorMessage);

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
                    _errorMessage = "\u26a0 Key must not be empty!";
                    return;
                }

                var soundGroups = FileHelper.LoadAssetsWithType<SoundGroupSO>();
                foreach (var soundGroup in soundGroups)
                {
                    foreach (var soundEntry in soundGroup.soundEntries)
                    {
                        if (soundEntry == this)
                            continue;

                        if (soundEntry.defineKeyConstName == defineKeyConstName)
                        {
                            _errorMessage = $"\u26a0 Duplicate define key const with other SoundEntry from SoundGroup: {soundGroup.name}!";
                            return;
                        }

                        if (soundEntry.key == key)
                        {
                            _errorMessage = $"\u26a0 Duplicate key with other SoundEntry from SoundGroup: {soundGroup.name}!";
                            return;
                        }
                    }
                }

                _errorMessage = string.Empty;
#endif
            }
        }
        
        #endregion
    }
}