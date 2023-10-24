using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class GoogleSheetConfigSO<T> : ScriptableObject where T : new()
    {
        [SerializeField] private string _sheetId;
        [SerializeField] private string _gridId;
        [SerializeField] private string _tsvCachePath;
        [SerializeField] private string _jsonCachePath;
        [SerializeField] protected List<T> _datas = new List<T>();

#if UNITY_EDITOR
        [ButtonMethod(ButtonMethodAttribute.EDrawOrder.BeforeInspector)]
        public void Sync() =>
            GoogleSheetHelper.GetConfig<T>(_sheetId, _gridId, OnSynced, _tsvCachePath, _jsonCachePath);

        protected virtual void OnSynced(List<T> googleSheetData)
        {
            _datas = googleSheetData;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
