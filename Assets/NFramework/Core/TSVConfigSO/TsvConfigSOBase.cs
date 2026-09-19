using System;
using System.IO;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public abstract class TsvConfigSOBase : ScriptableObject
    {
        [SerializeField] private string _category = "Default";
        [SerializeField] private string _tsvPath = "";
        [SerializeField] private bool _enableCache = false;
        [SerializeField, ShowIf(nameof(_enableCache))] private string _cachePath = "";
        [SerializeField, ReadOnly] private string _lastSyncedTime = "";

        public string Category
        {
            get => _category;
            set => _category = value;
        }

        public string TsvPath
        {
            get => _tsvPath;
            set => _tsvPath = value;
        }

        public bool EnableCache
        {
            get => _enableCache;
            set => _enableCache = value;
        }

        public string CachePath
        {
            get => _cachePath;
            set => _cachePath = value;
        }

        public string LastSyncedTime => _lastSyncedTime;

        public abstract void Fetch();

        public abstract Task FetchAsync();

        [Button("Fetch / Sync", ButtonSizes.Large), PropertyOrder(10)]
        public async void FetchFromInspector()
        {
            try
            {
                await FetchAsync();
                NLogger.Log($"[TSV] Successfully fetched {name}.");
            }
            catch (Exception ex)
            {
                NLogger.LogError($"[TSV] Failed to fetch {name}: {ex.Message}");
            }
        }

        [Button("Open Source", ButtonSizes.Medium), PropertyOrder(11)]
        public void OpenSource()
        {
            if (string.IsNullOrWhiteSpace(_tsvPath))
            {
                return;
            }

            if (TsvParser.IsWebUrl(_tsvPath))
            {
                Application.OpenURL(_tsvPath);
            }
            else if (File.Exists(_tsvPath))
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(_tsvPath);
#else
                Application.OpenURL(_tsvPath);
#endif
            }
        }

        public string GetDefaultCachePath()
        {
            return $"Assets/DataCache/TSV/{name}.tsv";
        }

        protected void UpdateLastSyncedTime()
        {
            _lastSyncedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
