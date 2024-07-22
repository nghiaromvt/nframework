using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class GoogleSheetConfigSO<T> : ScriptableObject where T : new()
    {
        [SerializeField] protected List<T> _datas = new List<T>();

#if UNITY_EDITOR
        [Header("Editor")]
        [SerializeField] protected string _sheetId;
        [SerializeField] protected string _gridId;
        [SerializeField] protected string _tsvCachePath;
        [SerializeField] protected string _jsonCachePath;

        [ButtonMethod(ButtonMethodAttribute.EDrawOrder.BeforeInspector)]
        public void OpenSheet() => 
            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{_sheetId}/edit#gid={_gridId}");

        [ButtonMethod(ButtonMethodAttribute.EDrawOrder.BeforeInspector)]
        protected void Sync() =>
            GoogleSheetHelper.GetConfig<T>(_sheetId, _gridId, OnSynced, _tsvCachePath, _jsonCachePath);
        
        protected virtual void OnSynced(List<T> googleSheetData)
        {
            _datas = googleSheetData;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #region Runtime
        [Header("Runtime")]
        [SerializeField] protected string _sheetIdRuntime;
        [SerializeField] protected string _gridIdRuntime;

        public void SyncRuntime(Action callback = null)
        {
            GoogleSheetHelper.GetConfig<T>(_sheetIdRuntime, _gridIdRuntime, data =>
            {
                OnSyncedRuntime(data);
                callback?.Invoke();
            });
        }

        protected virtual void OnSyncedRuntime(List<T> googleSheetData) => _datas = googleSheetData;
        #endregion
    }
}
