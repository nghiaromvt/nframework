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

        public virtual void OnSpawnedFromPool() => EventOnSpawnedFromPool?.Invoke();

        public virtual void OnBeforeReturnToPool() => EventOnBeforeReturnPool?.Invoke();

        public void ReturnToPool()
        {
            if (_pool)
                _pool.ReturnToPool(this);
            else
            {
                NLogger.LogError($"Pool is null. Destroying {name} instead.");
                Destroy(gameObject);
            }
        }
    }
}