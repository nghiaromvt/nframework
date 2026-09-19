using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public abstract class TsvConfigSO<T> : TsvConfigSOBase where T : class, new()
    {
        [SerializeField, Searchable] private List<T> _items = new();

        public IReadOnlyList<T> Items => _items;

        public override void Fetch()
        {
            var rawContent = TsvParser.FetchContent(TsvPath);
            ProcessRawTsv(rawContent);
        }

        public override async Task FetchAsync()
        {
            var rawContent = await TsvParser.FetchContentAsync(TsvPath);
            ProcessRawTsv(rawContent);
        }

        private void ProcessRawTsv(string rawContent)
        {
            if (EnableCache)
            {
                var cachePath = string.IsNullOrWhiteSpace(CachePath) ? GetDefaultCachePath() : CachePath;
                TsvParser.SaveToCache(cachePath, rawContent);
            }

            _items = TsvParser.Parse<T>(rawContent);
            UpdateLastSyncedTime();
            OnAfterFetched();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        protected virtual void OnAfterFetched()
        {
        }
    }
}
