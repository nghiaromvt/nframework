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
        public virtual void Sync()
        {
            GoogleSheetHelper.GetConfig<T>(_sheetId, _gridId, datas =>
            {
                _datas = datas;
                UnityEditor.EditorUtility.SetDirty(this);
            }, _tsvCachePath, _jsonCachePath);
        }
#endif
    }
}
