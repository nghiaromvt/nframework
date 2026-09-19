#if ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace NFramework
{
    public static class AddressablesLoader
    {
        private class Entry
        {
            public AsyncOperationHandle UnderlyingHandle;
            public object Result;
            public int RefCount;
            public UniTaskCompletionSource LoadTcs;
            public int WaiterCount;
        }

        private static readonly Dictionary<(string Key, Type Type), Entry> _cache = new();
        private static UniTaskCompletionSource _initTcs;
        private static bool _isInitialized;

        public static bool IsInitialized => _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _cache.Clear();
            _initTcs = null;
            _isInitialized = false;
        }

        static AddressablesLoader()
        {
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            if (_cache.Count > 0)
            {
                foreach (var kvp in _cache)
                {
                    NLogger.LogError($"[AddressablesLoader] Leaked asset on quit: Key='{kvp.Key.Key}', Type='{kvp.Key.Type}', RefCount={kvp.Value.RefCount}");
                }
            }
        }

        public static async UniTask Initialize()
        {
            if (_isInitialized) return;

            while (_initTcs != null)
            {
                await _initTcs.Task;
                if (_isInitialized) return;
            }

            _initTcs = new UniTaskCompletionSource();

            try
            {
                var handle = Addressables.InitializeAsync(false);
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    _isInitialized = true;
                    _initTcs.TrySetResult();
                }
                else
                {
                    var ex = new Exception("[AddressablesLoader] Addressables.InitializeAsync failed.");
                    _initTcs.TrySetException(ex);
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                NLogger.LogError($"[AddressablesLoader] Initialization error: {ex.Message}");
                _initTcs = null;
                throw;
            }
        }

        public static async UniTask<AssetHandle<T>> LoadAsset<T>(
            string key,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default) where T : Object
        {
            if (string.IsNullOrEmpty(key))
            {
                NLogger.LogError("[AddressablesLoader] Invalid null or empty key provided to LoadAsset.");
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            if (!_isInitialized)
            {
                await Initialize();
            }

            var cacheKey = (key, typeof(T));

            if (_cache.TryGetValue(cacheKey, out var existingEntry))
            {
                if (existingEntry.Result != null)
                {
                    existingEntry.RefCount++;
                    progress?.Report(1f);
                    return new AssetHandle<T>(key, (T)existingEntry.Result, () => Release(key, typeof(T)));
                }

                existingEntry.WaiterCount++;
                try
                {
                    await MonitorProgressAndAwait(existingEntry.UnderlyingHandle, existingEntry.LoadTcs, progress, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    existingEntry.WaiterCount--;
                    CheckAbandonOnCancel(cacheKey, existingEntry);
                    throw;
                }

                existingEntry.WaiterCount--;
                existingEntry.RefCount++;
                return new AssetHandle<T>(key, (T)existingEntry.Result, () => Release(key, typeof(T)));
            }

            var entry = new Entry
            {
                RefCount = 1,
                WaiterCount = 1,
                LoadTcs = new UniTaskCompletionSource()
            };

            var handle = Addressables.LoadAssetAsync<T>(key);
            entry.UnderlyingHandle = handle;
            _cache.Add(cacheKey, entry);

            ExecuteLoadAsync<T>(cacheKey, entry, handle).Forget();

            try
            {
                await MonitorProgressAndAwait(entry.UnderlyingHandle, entry.LoadTcs, progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                entry.WaiterCount--;
                entry.RefCount--;
                CheckAbandonOnCancel(cacheKey, entry);
                throw;
            }

            entry.WaiterCount--;
            return new AssetHandle<T>(key, (T)entry.Result, () => Release(key, typeof(T)));
        }

        public static async UniTask<AssetHandle<IList<T>>> LoadAssets<T>(
            string label,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default) where T : Object
        {
            if (string.IsNullOrEmpty(label))
            {
                NLogger.LogError("[AddressablesLoader] Invalid null or empty label provided to LoadAssets.");
                throw new ArgumentException("Label cannot be null or empty.", nameof(label));
            }

            if (!_isInitialized)
            {
                await Initialize();
            }

            var cacheKey = (label, typeof(IList<T>));

            if (_cache.TryGetValue(cacheKey, out var existingEntry))
            {
                if (existingEntry.Result != null)
                {
                    existingEntry.RefCount++;
                    progress?.Report(1f);
                    return new AssetHandle<IList<T>>(label, (IList<T>)existingEntry.Result, () => Release(label, typeof(IList<T>)));
                }

                existingEntry.WaiterCount++;
                try
                {
                    await MonitorProgressAndAwait(existingEntry.UnderlyingHandle, existingEntry.LoadTcs, progress, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    existingEntry.WaiterCount--;
                    CheckAbandonOnCancel(cacheKey, existingEntry);
                    throw;
                }

                existingEntry.WaiterCount--;
                existingEntry.RefCount++;
                return new AssetHandle<IList<T>>(label, (IList<T>)existingEntry.Result, () => Release(label, typeof(IList<T>)));
            }

            var entry = new Entry
            {
                RefCount = 1,
                WaiterCount = 1,
                LoadTcs = new UniTaskCompletionSource()
            };

            var handle = Addressables.LoadAssetsAsync<T>(label, null);
            entry.UnderlyingHandle = handle;
            _cache.Add(cacheKey, entry);

            ExecuteLoadAsync<IList<T>>(cacheKey, entry, handle).Forget();

            try
            {
                await MonitorProgressAndAwait(entry.UnderlyingHandle, entry.LoadTcs, progress, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                entry.WaiterCount--;
                entry.RefCount--;
                CheckAbandonOnCancel(cacheKey, entry);
                throw;
            }

            entry.WaiterCount--;
            return new AssetHandle<IList<T>>(label, (IList<T>)entry.Result, () => Release(label, typeof(IList<T>)));
        }

        private static async UniTaskVoid ExecuteLoadAsync<TResult>(
            (string Key, Type Type) cacheKey,
            Entry entry,
            AsyncOperationHandle handle)
        {
            try
            {
                await handle.ToUniTask();

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    entry.Result = handle.Result;
                    entry.LoadTcs.TrySetResult();

                    if (entry.RefCount <= 0 && entry.WaiterCount <= 0)
                    {
                        _cache.Remove(cacheKey);
                        if (handle.IsValid())
                        {
                            Addressables.Release(handle);
                        }
                    }
                }
                else
                {
                    _cache.Remove(cacheKey);
                    var ex = new Exception($"[AddressablesLoader] Failed to load Addressable asset for key: '{cacheKey.Key}', Type: '{cacheKey.Type}'.");
                    NLogger.LogError(ex.Message);
                    entry.LoadTcs.TrySetException(ex);
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }
            }
            catch (Exception ex)
            {
                _cache.Remove(cacheKey);
                NLogger.LogError($"[AddressablesLoader] Exception while loading asset '{cacheKey.Key}': {ex.Message}");
                entry.LoadTcs.TrySetException(ex);
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        private static async UniTask MonitorProgressAndAwait(
            AsyncOperationHandle handle,
            UniTaskCompletionSource tcs,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            if (progress == null)
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
                return;
            }

            while (!handle.IsDone && !tcs.Task.Status.IsCompleted())
            {
                progress.Report(handle.PercentComplete);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            await tcs.Task.AttachExternalCancellation(cancellationToken);
            progress.Report(1f);
        }

        private static void CheckAbandonOnCancel((string Key, Type Type) cacheKey, Entry entry)
        {
            if (entry.WaiterCount <= 0 && entry.RefCount <= 0 && entry.UnderlyingHandle.IsDone)
            {
                _cache.Remove(cacheKey);
                if (entry.UnderlyingHandle.IsValid())
                {
                    Addressables.Release(entry.UnderlyingHandle);
                }
            }
        }

        public static void Release(string key, Type type)
        {
            var cacheKey = (key, type);
            if (!_cache.TryGetValue(cacheKey, out var entry))
            {
                NLogger.LogError($"[AddressablesLoader] Attempted to release non-cached asset: Key='{key}', Type='{type}'.");
                return;
            }

            entry.RefCount--;
            if (entry.RefCount <= 0)
            {
                _cache.Remove(cacheKey);
                if (entry.UnderlyingHandle.IsValid())
                {
                    Addressables.Release(entry.UnderlyingHandle);
                }
            }
        }

        public static bool IsAssetLoaded(string key, Type type)
        {
            return _cache.TryGetValue((key, type), out var entry) && entry.Result != null;
        }
    }
}
#endif
