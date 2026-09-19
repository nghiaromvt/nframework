using System;
using UnityEngine;

namespace NFramework
{
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour
    {
        private IPool _pool;
        private Component _target;
        private IPoolable[] _poolables;

        public IPool Pool => _pool;
        public Component Target => _target;
        internal IPoolable[] Poolables => _poolables;

        private void OnDestroy()
        {
            if (_pool != null)
            {
                _pool.OnInstanceDestroyed(_target != null ? _target : this);
            }
        }

        internal void Initialize(IPool pool, Component target, IPoolable[] poolables)
        {
            _pool = pool;
            _target = target;
            _poolables = poolables;
        }

        public void Release()
        {
            if (_pool == null)
            {
                throw new InvalidOperationException($"PooledObject on '{gameObject.name}' has no owning pool assigned or the pool has been cleared.");
            }

            _pool.Despawn(_target != null ? _target : this);
        }

        public void ReleaseVoid()
        {
            Release();
        }
    }
}
