using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    [Serializable]
    public class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector] private List<TKey> _keys = new();
        [SerializeField, HideInInspector] private List<TValue> _values = new();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < _keys.Count && i < _values.Count; i++)
            {
                this[_keys[i]] = _values[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var item in this)
            {
                _keys.Add(item.Key);
                _values.Add(item.Value);
            }
        }
    }
}



