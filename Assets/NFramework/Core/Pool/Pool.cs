using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public class Pool<T> : IPool where T : Component
    {
        private readonly T _prefab;
        private readonly int _maxSize;
        private readonly PoolScope _scope;
        private readonly GameObject _rootGameObject;
        private readonly Transform _rootTransform;
        private readonly GameObject _inactiveContainer;
        private readonly PoolRootMarker _rootMarker;
        private readonly ObjectPool<T> _objectPool;
        private readonly HashSet<T> _activeObjects = new();
        private readonly Dictionary<T, PooledObject> _cachedPooledObjects = new();
        private readonly List<T> _prewarmPendingList = new();

        private CancellationTokenSource _lifetimeCts = new();
        private bool _isPrewarming;
        private bool _isDisposed;
        private bool _isDisposing;

        public T Prefab => _prefab;
        public Component IPoolPrefab => _prefab;
        Component IPool.Prefab => _prefab;
        public int ActiveCount => _activeObjects.Count;
        public int IdleCount => _objectPool.CountInactive;
        public int MaxSize => _maxSize;
        public PoolScope Scope => _scope;
        public GameObject RootGameObject => _rootGameObject;

        public Pool(T prefab, int prewarm = 0, int maxSize = 10, PoolScope scope = PoolScope.Scene, Scene scene = default)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "maxSize must be greater than zero.");
            }

            if (prewarm < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(prewarm), "prewarm count cannot be negative.");
            }

            if (prewarm > maxSize)
            {
                throw new InvalidOperationException($"Initial prewarm count ({prewarm}) cannot exceed maxSize ({maxSize}).");
            }

            _prefab = prefab;
            _maxSize = maxSize;
            _scope = scope;

            _rootGameObject = new GameObject($"PoolRoot_{_prefab.name}");
            _rootTransform = _rootGameObject.transform;

            _inactiveContainer = new GameObject("InactiveContainer");
            _inactiveContainer.transform.SetParent(_rootTransform, false);
            _inactiveContainer.SetActive(false);

            _rootMarker = _rootGameObject.AddComponent<PoolRootMarker>();
            _rootMarker.Initialize(Dispose);

            if (_scope == PoolScope.Global)
            {
                UnityEngine.Object.DontDestroyOnLoad(_rootGameObject);
            }
            else if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(_rootGameObject, scene);
            }

            _objectPool = new ObjectPool<T>(
                createFunc: CreateInstance,
                actionOnGet: null,
                actionOnRelease: null,
                actionOnDestroy: DestroyInstance,
                collectionCheck: false,
                defaultCapacity: Mathf.Max(1, prewarm),
                maxSize: maxSize
            );

            if (prewarm > 0)
            {
                Prewarm(prewarm);
            }
        }

        public T Spawn()
        {
            return Spawn(Vector3.zero, Quaternion.identity, null);
        }

        public T Spawn(Transform parent)
        {
            Vector3 position = parent != null ? parent.position : Vector3.zero;
            Quaternion rotation = parent != null ? parent.rotation : Quaternion.identity;
            return Spawn(position, rotation, parent);
        }

        public T Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException($"Cannot spawn from a disposed pool for prefab '{_prefab.name}'.");
            }

            T instance;
            if (_prewarmPendingList.Count > 0)
            {
                int lastIndex = _prewarmPendingList.Count - 1;
                instance = _prewarmPendingList[lastIndex];
                _prewarmPendingList.RemoveAt(lastIndex);
            }
            else
            {
                instance = _objectPool.Get();
            }

            _activeObjects.Add(instance);

            Transform instanceTransform = instance.transform;
            if (parent != null)
            {
                instanceTransform.SetParent(parent, false);
            }
            else
            {
                instanceTransform.SetParent(_rootTransform, false);
            }

            instanceTransform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);

            if (_cachedPooledObjects.TryGetValue(instance, out var pooledObj) && pooledObj.Poolables != null)
            {
                var poolables = pooledObj.Poolables;
                for (int i = 0; i < poolables.Length; i++)
                {
                    poolables[i].OnSpawn();
                }
            }

            return instance;
        }

        public void Despawn(T instance)
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException($"Cannot despawn into a disposed pool for prefab '{_prefab.name}'.");
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!_activeObjects.Remove(instance))
            {
                throw new InvalidOperationException($"Attempted to despawn an instance of '{instance.name}' that is not active in this pool (double return or unmanaged object).");
            }

            if (_cachedPooledObjects.TryGetValue(instance, out var pooledObj) && pooledObj.Poolables != null)
            {
                var poolables = pooledObj.Poolables;
                for (int i = 0; i < poolables.Length; i++)
                {
                    poolables[i].OnDespawn();
                }
            }

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(_rootTransform, false);
            instance.gameObject.SetActive(false);

            _objectPool.Release(instance);
        }

        public void Despawn(Component instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (instance is T typedInstance)
            {
                Despawn(typedInstance);
            }
            else if (instance is PooledObject pooledObj && pooledObj.Target is T targetTyped)
            {
                Despawn(targetTyped);
            }
            else if (instance.TryGetComponent<T>(out var componentInstance))
            {
                Despawn(componentInstance);
            }
            else
            {
                throw new InvalidOperationException($"Component '{instance.GetType().Name}' on '{instance.name}' is not compatible with pool type '{typeof(T).Name}'.");
            }
        }

        public void Prewarm(int count)
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException($"Cannot prewarm a disposed pool for prefab '{_prefab.name}'.");
            }

            if (count > _maxSize)
            {
                throw new InvalidOperationException($"Cannot prewarm {count} items. Pool max size is {_maxSize}.");
            }

            if (_isPrewarming)
            {
                throw new InvalidOperationException($"Cannot run synchronous Prewarm while an async prewarm is already in progress for prefab '{_prefab.name}'.");
            }

            int itemsNeeded = count - IdleCount;
            if (itemsNeeded <= 0)
            {
                return;
            }

            var temp = ListPool<T>.Get();
            try
            {
                for (int i = 0; i < itemsNeeded; i++)
                {
                    temp.Add(_objectPool.Get());
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    _objectPool.Release(temp[i]);
                }
            }
            finally
            {
                ListPool<T>.Release(temp);
            }
        }

        public async UniTask PrewarmAsync(int count, int perFrame, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException($"Cannot prewarm a disposed pool for prefab '{_prefab.name}'.");
            }

            if (count > _maxSize)
            {
                throw new InvalidOperationException($"Cannot prewarm {count} items. Pool max size is {_maxSize}.");
            }

            if (perFrame <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perFrame), "perFrame must be greater than zero.");
            }

            if (_isPrewarming)
            {
                throw new InvalidOperationException($"An async prewarm operation is already in progress for prefab '{_prefab.name}'.");
            }

            _isPrewarming = true;
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _lifetimeCts.Token);
            CancellationToken linkedToken = linkedCts.Token;

            try
            {
                int itemsNeeded = count - IdleCount - _prewarmPendingList.Count;
                int createdThisFrame = 0;

                while (itemsNeeded > 0)
                {
                    linkedToken.ThrowIfCancellationRequested();

                    T item = _objectPool.Get();
                    _prewarmPendingList.Add(item);
                    itemsNeeded--;
                    createdThisFrame++;

                    if (createdThisFrame >= perFrame && itemsNeeded > 0)
                    {
                        createdThisFrame = 0;
                        await UniTask.Yield(cancellationToken: linkedToken);
                    }
                }
            }
            finally
            {
                for (int i = 0; i < _prewarmPendingList.Count; i++)
                {
                    if (_prewarmPendingList[i] != null)
                    {
                        _objectPool.Release(_prewarmPendingList[i]);
                    }
                }

                _prewarmPendingList.Clear();
                _isPrewarming = false;
            }
        }

        public void DespawnAll()
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException($"Cannot DespawnAll on a disposed pool for prefab '{_prefab.name}'.");
            }

            if (_activeObjects.Count == 0)
            {
                return;
            }

            var snapshot = ListPool<T>.Get();
            try
            {
                foreach (var active in _activeObjects)
                {
                    snapshot.Add(active);
                }

                for (int i = 0; i < snapshot.Count; i++)
                {
                    Despawn(snapshot[i]);
                }
            }
            finally
            {
                ListPool<T>.Release(snapshot);
            }
        }

        public void OnInstanceDestroyed(Component instance)
        {
            if (_isDisposing || _isDisposed)
            {
                return;
            }

            if (instance is T typedInstance)
            {
                _activeObjects.Remove(typedInstance);
                _cachedPooledObjects.Remove(typedInstance);
            }
            else if (instance is PooledObject pooledObj && pooledObj.Target is T targetTyped)
            {
                _activeObjects.Remove(targetTyped);
                _cachedPooledObjects.Remove(targetTyped);
            }
        }

        public void Clear()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _isDisposing = true;

            if (_lifetimeCts != null)
            {
                _lifetimeCts.Cancel();
                _lifetimeCts.Dispose();
                _lifetimeCts = null;
            }

            foreach (var active in _activeObjects)
            {
                if (active != null)
                {
                    SafeDestroy(active.gameObject);
                }
            }

            _activeObjects.Clear();
            _cachedPooledObjects.Clear();

            for (int i = 0; i < _prewarmPendingList.Count; i++)
            {
                if (_prewarmPendingList[i] != null)
                {
                    SafeDestroy(_prewarmPendingList[i].gameObject);
                }
            }

            _prewarmPendingList.Clear();

            _objectPool.Clear();

            if (_rootMarker != null)
            {
                _rootMarker.Clear();
            }

            if (_rootGameObject != null)
            {
                SafeDestroy(_rootGameObject);
            }

            PoolService.UnregisterInternal(_prefab);
        }

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _inactiveContainer.transform);
            GameObject go = instance.gameObject;
            go.SetActive(false);
            go.transform.SetParent(_rootTransform, false);

            if (!go.TryGetComponent<PooledObject>(out var pooledObj))
            {
                pooledObj = go.AddComponent<PooledObject>();
            }

            IPoolable[] poolables = go.GetComponents<IPoolable>();
            pooledObj.Initialize(this, instance, poolables);
            _cachedPooledObjects[instance] = pooledObj;

            return instance;
        }

        private void DestroyInstance(T instance)
        {
            if (instance != null)
            {
                _cachedPooledObjects.Remove(instance);
                SafeDestroy(instance.gameObject);
            }
        }

        private static void SafeDestroy(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }
    }
}
