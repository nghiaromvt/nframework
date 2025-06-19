using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class Pool : MonoBehaviour
    {
        public static Pool CreatePool(bool initializeAtAwake, bool autoExpandPool, int initPoolSize, PooledObject objectToPool, int maxPoolSize = -1)
        {
            var go = new GameObject($"Pool_{objectToPool.name}_{objectToPool.GetInstanceID()}", typeof(Pool));
            var pool = go.GetComponent<Pool>();
            pool._autoExpandPool = autoExpandPool;
            pool._initPoolSize = initPoolSize;
            pool._objectToPool = objectToPool;
            pool._maxPoolSize = maxPoolSize;
            pool._initializeAtAwake = initializeAtAwake;

            if (initializeAtAwake)
                pool.InitializePool();

            return pool;
        }

        [SerializeField] private bool _initializeAtAwake;
        [SerializeField] private bool _autoExpandPool = true;
        [SerializeField, ShowIf(nameof(_autoExpandPool))] private int _maxPoolSize = -1;
        [SerializeField] private int _initPoolSize = 5;
        [SerializeField] private PooledObject _objectToPool;

        private readonly Queue<PooledObject> _poolQueue = new();
        private readonly List<PooledObject> _activeObjects = new();

        public PooledObject ObjectToPool => _objectToPool;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (_initializeAtAwake)
                InitializePool();
        }

        public void InitializePool()
        {
            if (IsInitialized) return;
            
            if (_objectToPool == null)
            {
                NLogger.LogWarning("Object to pool is not assigned.", this);
                return;
            }

            IsInitialized = true;
            
            for (int i = 0; i < _initPoolSize; i++)
            {
                var instance = Instantiate(_objectToPool, transform);
                instance.Pool = this;
                instance.gameObject.SetActive(false);
                instance.name += $"_{i}";
                _poolQueue.Enqueue(instance);
            }
        }

        public PooledObject SpawnPooledObject(PooledObjectInputData inputData = null)
        {
            if (_poolQueue.Count > 0)
            {
                var instance = _poolQueue.Dequeue();
                instance.gameObject.SetActive(true);
                _activeObjects.Add(instance);
                instance.OnSpawnedFromPool(inputData);
                return instance;
            }

            int totalInstances = _activeObjects.Count + _poolQueue.Count;
            if (_autoExpandPool && (_maxPoolSize <= 0 || totalInstances < _maxPoolSize))
            {
                var instance = Instantiate(_objectToPool, transform);
                instance.Pool = this;
                instance.name += $"_{totalInstances}";
                instance.gameObject.SetActive(true);
                _activeObjects.Add(instance);
                instance.OnSpawnedFromPool(inputData);
                return instance;
            }

            NLogger.LogWarning("Pool is empty and cannot expand further.", this);
            return null;
        }

        public PooledObjectOutputData ReturnToPool(PooledObject pooledObject)
        {
            if (pooledObject.Pool != this)
            {
                NLogger.LogError($"Cannot return {pooledObject.name} to pool – not from this pool.", this);
                return null;
            }

            if (!_activeObjects.Remove(pooledObject))
            {
                NLogger.LogError($"Attempted to return {pooledObject.name}, but it was not in the active list.", this);
                return null;
            }

            var outputData = pooledObject.OnBeforeReturnToPool();

            int totalInstances = _poolQueue.Count + _activeObjects.Count;
            if (_autoExpandPool && _maxPoolSize > 0 && totalInstances >= _maxPoolSize)
            {
                Destroy(pooledObject.gameObject);
            }
            else
            {
                pooledObject.gameObject.SetActive(false);
                pooledObject.transform.SetParent(transform);
                _poolQueue.Enqueue(pooledObject);
            }
            
            return outputData;
        }

        public void ReturnAllToPool()
        {
            var activeCopy = new List<PooledObject>(_activeObjects);
            foreach (var pooledObject in activeCopy)
            {
                ReturnToPool(pooledObject);
            }
        }

        public void HandlePooledObjectOnDestroy(PooledObject pooledObject)
        {
            _activeObjects.Remove(pooledObject);
        }
    }
}
