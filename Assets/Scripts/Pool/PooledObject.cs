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
                    Logger.LogError($"Cannot set pool, because it had already set", this);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Pool)
                Pool.HandlePooledObjectOnDestroy(this);
        }

        public virtual void OnSpawnedFromPool() => EventOnSpawnedFromPool?.Invoke();

        public virtual void OnBeforeReturnToPool() => EventOnBeforeReturnPool?.Invoke();

        public void ReturnToPool()
        {
            if (Pool)
            {
                Pool.ReturnToPool(this);
            }
            else
            {
                Logger.LogError($"Pool is null => Destroy!");
                Destroy(gameObject);
            }
        }
    }
}