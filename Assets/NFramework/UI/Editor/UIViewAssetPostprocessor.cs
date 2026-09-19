using System.Collections.Generic;
using UnityEditor;
#if ADDRESSABLES
using UnityEditor.AddressableAssets.Settings;
#endif

namespace NFramework.UI.Editor
{
    [InitializeOnLoad]
    public class UIViewAssetPostprocessor : AssetPostprocessor
    {
        static UIViewAssetPostprocessor()
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
                UpdateUIViewFromGuid(singleEntry.guid);
            }
            else if (eventData is IEnumerable<AddressableAssetEntry> entries)
            {
                foreach (var entry in entries)
                {
                    UpdateUIViewFromGuid(entry.guid);
                }
            }
            else if (eventData is IEnumerable<string> guids)
            {
                foreach (var guid in guids)
                {
                    UpdateUIViewFromGuid(guid);
                }
            }
            else if (eventData is string singleGuid)
            {
                UpdateUIViewFromGuid(singleGuid);
            }
        }

        private static void UpdateUIViewFromGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".prefab"))
                return;

            var view = AssetDatabase.LoadAssetAtPath<UIView>(path);
            if (view != null)
            {
                view.RefreshKey();
                view.ValidateKey();
                EditorUtility.SetDirty(view);
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
                if (!path.EndsWith(".prefab"))
                    continue;

                var view = AssetDatabase.LoadAssetAtPath<UIView>(path);
                if (view != null)
                {
                    view.RefreshKey();
                    view.ValidateKey();
                    EditorUtility.SetDirty(view);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
