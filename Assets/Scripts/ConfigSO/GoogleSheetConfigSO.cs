using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class GoogleSheetConfigSO<T> : ScriptableObject where T : new()
    {
        [SerializeField, Searchable] protected List<T> _datas = new();

#if UNITY_EDITOR
        [Header("Info")]
        [SerializeField] protected string _sheetId;
        [SerializeField] protected bool _isSheetIdEncrypted;
        [SerializeField] protected string _gridId;
        [SerializeField] protected string _tsvCachePath;
        [SerializeField] protected string _jsonCachePath;

        [Button(ButtonSizes.Gigantic)]
        public void OpenSheet()
        {
            var sheeId = _sheetId;
            if (_isSheetIdEncrypted)
                sheeId = AesHelper.DecryptAes(sheeId, AesHelper.GetGlobalEditorAesKey());

            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{sheeId}/edit#gid={_gridId}");
        }

        [Button(ButtonSizes.Gigantic)]
        protected void Sync()
        {
            var sheeId = _sheetId;
            if (_isSheetIdEncrypted)
                sheeId = AesHelper.DecryptAes(sheeId, AesHelper.GetGlobalEditorAesKey());

            GoogleSheetHelper.GetConfig<T>(sheeId, _gridId, OnSynced, _tsvCachePath, _jsonCachePath);
        }

        protected virtual void OnSynced(List<T> googleSheetData)
        {
            _datas = googleSheetData;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}