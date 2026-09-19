using System;
using UnityEngine;

namespace NFramework
{
    [DisallowMultipleComponent]
    public class PoolRootMarker : MonoBehaviour
    {
        private Action _onDestroyAction;

        private void OnDestroy()
        {
            _onDestroyAction?.Invoke();
        }

        internal void Initialize(Action onDestroyAction)
        {
            _onDestroyAction = onDestroyAction;
        }

        internal void Clear()
        {
            _onDestroyAction = null;
        }
    }
}
