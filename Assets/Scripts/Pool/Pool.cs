using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class Pool : MonoBehaviour
    {
        public static Pool CreatePool(bool initializeAtAwake, bool autoExpandPool, int initPoolSize, PooledObject objectToPool)
        {
            var go = new GameObject($"Pool_{objectToPool.name}", typeof(Pool));
            var pool = go.GetComponent<Pool>();
            pool._autoExpandPool = autoExpandPool;
            pool._initPoolSize = initPoolSize;
            pool._initPoolSize = initPoolSize;
            pool._objectToPool = objectToPool;
            
            pool._initializeAtAwake = initializeAtAwake;
            if (initializeAtAwake)
                pool.InitializePool();

            return pool;
        }

        [SerializeField] private bool _initializeAtAwake;
        [SerializeField] private bool _autoExpandPool = true;
        [SerializeField] private int _initPoolSize = 5;
        [SerializeField] private PooledObject _objectToPool;

        private Queue<PooledObject> _poolQueue = new Queue<PooledObject>();
        private List<PooledObject> _activeObjects = new List<PooledObject>();

        public PooledObject ObjectToPool => _objectToPool;

        private void Awake()
        {
            if (_initializeAtAwake)
                InitializePool();
        }

        public void InitializePool()
        {
            if (_objectToPool == null)
                return;

            for (int i = 0; i < _initPoolSize; i++)
            {
                var instance = Instantiate(_objectToPool, transform);
                instance.Pool = this;
                instance.gameObject.SetActive(false);
                _poolQueue.Enqueue(instance);
            }
        }

        public PooledObject GetPooledObject()
        {
            if (_poolQueue.Count == 0)
            {
                if (_autoExpandPool)
                {
                    var instance = Instantiate(_objectToPool, transform);
                    instance.Pool = this;
                    instance.gameObject.SetActive(true);
                    _activeObjects.Add(instance);
                    return instance;
                }
                else if (_activeObjects.Count > 0)
                {
                    var instance = _activeObjects[0];
                    _activeObjects.RemoveAt(0);
                    _activeObjects.Add(instance);
                    return instance;
                }
                else
                {
                    Logger.LogError($"Cannot get PooledObject", this);
                    return null;
                }
            }
            else
            {
                var instance = _poolQueue.Dequeue();
                instance.gameObject.SetActive(true);
                _activeObjects.Add(instance);
                return instance;
            }
        }

        public void ReturnToPool(PooledObject pooledObject)
        {
            if (pooledObject.Pool != this)
            {
                Logger.LogError($"Cannot return {pooledObject.name} to pool, because it's not in this pool", this);
                return;
            }

            pooledObject.gameObject.SetActive(false);
            pooledObject.transform.SetParent(transform);
            _poolQueue.Enqueue(pooledObject);

            if (_activeObjects.Contains(pooledObject))
                _activeObjects.Remove(pooledObject);
            else
                Logger.LogError($"Something went wrong! {pooledObject.name} isn't in activeObjects", this);
        }

        public void ReturnAllToPool()
        {
            var tempActiveObjects = new List<PooledObject>(_activeObjects);
            foreach (var pooledObject in tempActiveObjects)
                ReturnToPool(pooledObject);
        }

        public void HandlePooledObjectOnDestroy(PooledObject pooledObject)
        {
            if (_activeObjects.Contains(pooledObject))
                _activeObjects.Remove(pooledObject);
        }
    }
}