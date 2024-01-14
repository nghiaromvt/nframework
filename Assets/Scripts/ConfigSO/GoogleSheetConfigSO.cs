using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class GoogleSheetConfigSO<T> : ScriptableObject where T : new()
    {
        [SerializeField] protected string _sheetId;
        [SerializeField] protected string _gridId;
        [SerializeField] protected string _tsvCachePath;
        [SerializeField] protected string _jsonCachePath;
        [SerializeField] protected List<T> _datas = new List<T>();

        [ButtonMethod(ButtonMethodAttribute.EDrawOrder.BeforeInspector)]
        public void OpenSheet() => 
            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{_sheetId}/edit#gid={_gridId}");

        [ButtonMethod(ButtonMethodAttribute.EDrawOrder.BeforeInspector)]
        protected void Sync() =>
            GoogleSheetHelper.GetConfig<T>(_sheetId, _gridId, OnSynced, _tsvCachePath, _jsonCachePath);

        public void SyncRuntime(Action callback = null)
        {
            GoogleSheetHelper.GetConfig<T>(_sheetId, _gridId, data =>
            {
                OnSynced(data);
                callback?.Invoke();
            });
        }

        protected virtual void OnSynced(List<T> googleSheetData)
        {
            _datas = googleSheetData;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
