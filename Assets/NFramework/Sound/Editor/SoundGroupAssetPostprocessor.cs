using System.Collections.Generic;
using UnityEditor;
#if ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
using NFramework.Sound;
#endif

namespace NFramework.Sound.Editor
{
    [InitializeOnLoad]
    public class SoundGroupAssetPostprocessor : AssetPostprocessor
    {
        static SoundGroupAssetPostprocessor()
        {
#if ADDRESSABLES
            AddressableAssetSettings.OnModificationGlobal -= OnAddressablesModified;
            AddressableAssetSettings.OnModificationGlobal += OnAddressablesModified;
#endif
        }

#if ADDRESSABLES
        private static void OnAddressablesModified(
            AddressableAssetSettings settings,
            AddressableAssetSettings.ModificationEvent modEvent,
            object eventData)
        {
            switch (modEvent)
            {
                case AddressableAssetSettings.ModificationEvent.EntryCreated:
                case AddressableAssetSettings.ModificationEvent.EntryAdded:
                case AddressableAssetSettings.ModificationEvent.EntryModified:
                case AddressableAssetSettings.ModificationEvent.EntryMoved:
                case AddressableAssetSettings.ModificationEvent.EntryRemoved:
                    HandleAddressableEntryEvent(eventData);
                    break;
            }
        }

        private static void HandleAddressableEntryEvent(object eventData)
        {
            if (eventData is AddressableAssetEntry singleEntry)
            {
                UpdateSoundGroupFromGuid(singleEntry.guid);
            }
            else if (eventData is IEnumerable<AddressableAssetEntry> entries)
            {
                foreach (var entry in entries)
                {
                    UpdateSoundGroupFromGuid(entry.guid);
                }
            }
            else if (eventData is IEnumerable<string> guids)
            {
                foreach (var guid in guids)
                {
                    UpdateSoundGroupFromGuid(guid);
                }
            }
            else if (eventData is string singleGuid)
            {
                UpdateSoundGroupFromGuid(singleGuid);
            }
        }

        private static void UpdateSoundGroupFromGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".asset"))
                return;

            var soundGroup = AssetDatabase.LoadAssetAtPath<SoundGroupSO>(path);
            if (soundGroup != null)
            {
                soundGroup.RefreshKey();
                soundGroup.ValidateKey();
                EditorUtility.SetDirty(soundGroup);
                AssetDatabase.SaveAssets();
            }
        }
#endif

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in movedAssets)
            {
                if (!path.EndsWith(".asset"))
                    continue;

                var soundGroup = AssetDatabase.LoadAssetAtPath<SoundGroupSO>(path);
                if (soundGroup != null)
                {
                    soundGroup.RefreshKey();
                    soundGroup.ValidateKey();
                    EditorUtility.SetDirty(soundGroup);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
