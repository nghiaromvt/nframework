using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public static class PoolService
    {
        public const int DEFAULT_MAX_SIZE = 10;

        private static readonly Dictionary<Component, IPool> _pools = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            _pools.Clear();
        }

        public static bool HasPool(Component prefab)
        {
            return prefab != null && _pools.ContainsKey(prefab);
        }

        public static IPool GetPool(Component prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (_pools.TryGetValue(prefab, out var pool))
            {
                return pool;
            }

            throw new InvalidOperationException($"No pool registered for prefab '{prefab.name}'.");
        }

        public static Pool<T> GetPool<T>(T prefab) where T : Component
        {
            return (Pool<T>)GetPool((Component)prefab);
        }

        public static Pool<T> Register<T>(
            T prefab,
            int prewarm = 0,
            int maxSize = DEFAULT_MAX_SIZE,
            PoolScope scope = PoolScope.Scene,
            Scene scene = default) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (_pools.ContainsKey(prefab))
            {
                throw new InvalidOperationException($"A pool has already been registered for prefab '{prefab.name}'.");
            }

            var pool = new Pool<T>(prefab, prewarm, maxSize, scope, scene);
            _pools[prefab] = pool;
            return pool;
        }

        public static T Spawn<T>(T prefab) where T : Component
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null);
        }

        public static T Spawn<T>(T prefab, Transform parent) where T : Component
        {
            Vector3 position = parent != null ? parent.position : Vector3.zero;
            Quaternion rotation = parent != null ? parent.rotation : Quaternion.identity;
            return Spawn(prefab, position, rotation, parent);
        }

        public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = Register(prefab, prewarm: 0, maxSize: DEFAULT_MAX_SIZE, scope: PoolScope.Scene);
            }

            return ((Pool<T>)pool).Spawn(position, rotation, parent);
        }

        public static void Despawn<T>(T instance) where T : Component
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Instance cannot be null.");
            }

            if (!instance.TryGetComponent<PooledObject>(out var pooledObject) || pooledObject.Pool == null)
            {
                throw new InvalidOperationException($"Instance '{instance.name}' has no valid PooledObject component or owning pool.");
            }

            pooledObject.Release();
        }

        public static void DespawnAll(Component prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.DespawnAll();
            }
            else
            {
                throw new InvalidOperationException($"No pool registered for prefab '{prefab.name}'.");
            }
        }

        public static void DespawnAll()
        {
            var poolList = ListPool<IPool>.Get();
            try
            {
                foreach (var pool in _pools.Values)
                {
                    poolList.Add(pool);
                }

                for (int i = 0; i < poolList.Count; i++)
                {
                    poolList[i].DespawnAll();
                }
            }
            finally
            {
                ListPool<IPool>.Release(poolList);
            }
        }

        public static async UniTask PrewarmAsync<T>(
            T prefab,
            int count,
            int perFrame,
            CancellationToken cancellationToken = default) where T : Component
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = Register(prefab, prewarm: 0, maxSize: Mathf.Max(count, DEFAULT_MAX_SIZE), scope: PoolScope.Scene);
            }

            await ((Pool<T>)pool).PrewarmAsync(count, perFrame, cancellationToken);
        }

        public static void Clear(Component prefab)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null.");
            }

            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Clear();
            }
        }

        public static void ClearAll()
        {
            var poolList = ListPool<IPool>.Get();
            try
            {
                foreach (var pool in _pools.Values)
                {
                    poolList.Add(pool);
                }

                for (int i = 0; i < poolList.Count; i++)
                {
                    poolList[i].Clear();
                }
            }
            finally
            {
                ListPool<IPool>.Release(poolList);
            }

            _pools.Clear();
        }

        public static void Unregister(Component prefab)
        {
            if (prefab == null)
            {
                return;
            }

            _pools.Remove(prefab);
        }

        internal static void UnregisterInternal(Component prefab)
        {
            if (prefab != null)
            {
                _pools.Remove(prefab);
            }
        }
    }
}
