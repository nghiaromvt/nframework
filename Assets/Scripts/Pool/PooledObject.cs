using System;
using UnityEngine;
using UnityEngine.Events;

namespace NFramework
{
    public class PooledObject : MonoBehaviour
    {
        public UnityEvent EventOnSpawnedFromPool;
        public UnityEvent EventOnBeforeReturnPool;

        private Pool _pool;

        public Pool Pool
        {
            get => _pool;
            set
            {
                if (_pool == null)
                    _pool = value;
                else
                    NLogger.LogError($"Cannot set pool for {name}; it is already assigned.", this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_pool)
                _pool.HandlePooledObjectOnDestroy(this);
        }

        public virtual void OnSpawnedFromPool(PooledObjectInputData inputData) => EventOnSpawnedFromPool?.Invoke();

        public virtual PooledObjectOutputData OnBeforeReturnToPool()
        {
            EventOnBeforeReturnPool?.Invoke();
            return null;
        }

        public PooledObjectOutputData ReturnToPool()
        {
            if (_pool)
            {
                return _pool.ReturnToPool(this);
            }
            else
            {
                NLogger.LogError($"Pool is null. Destroying {name} instead.");
                Destroy(gameObject);
                return null;
            }
        }
    }
    
    [Serializable]
    public class PooledObjectInputData
    {

    }

    [Serializable]
    public class PooledObjectOutputData
    {

    }
}