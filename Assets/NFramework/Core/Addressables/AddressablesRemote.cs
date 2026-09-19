#if ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NFramework
{
    public static class AddressablesRemote
    {
        private class DownloadEntry
        {
            public AsyncOperationHandle Handle;
            public Action<long, long, float> ProgressCallback;
            public UniTaskCompletionSource<bool> Tcs;
            public CancellationTokenSource InternalCts;
        }

        private static readonly Dictionary<string, DownloadEntry> _activeDownloads = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _activeDownloads.Clear();
        }

        public static async UniTask<List<IResourceLocator>> UpdateCatalogs(bool autoCleanBundleCache = true)
        {
            if (!AddressablesLoader.IsInitialized)
            {
                await AddressablesLoader.Initialize();
            }

            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            await checkHandle.ToUniTask();

            if (checkHandle.Status != AsyncOperationStatus.Succeeded)
            {
                NLogger.LogError("[AddressablesRemote] CheckForCatalogUpdates failed.");
                if (checkHandle.IsValid()) Addressables.Release(checkHandle);
                return null;
            }

            var catalogsToUpdate = checkHandle.Result;
            Addressables.Release(checkHandle);

            if (catalogsToUpdate == null || catalogsToUpdate.Count == 0)
            {
                return new List<IResourceLocator>();
            }

            var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, autoCleanBundleCache);
            await updateHandle.ToUniTask();

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var result = updateHandle.Result;
                Addressables.Release(updateHandle);
                return result;
            }

            NLogger.LogError("[AddressablesRemote] UpdateCatalogs failed.");
            if (updateHandle.IsValid()) Addressables.Release(updateHandle);
            return null;
        }

        public static async UniTask<bool> IsAssetsDownloaded(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                NLogger.LogError("[AddressablesRemote] Invalid key provided to IsAssetsDownloaded.");
                return false;
            }

            if (!AddressablesLoader.IsInitialized)
            {
                await AddressablesLoader.Initialize();
            }

            var handle = Addressables.GetDownloadSizeAsync(key);
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                long size = handle.Result;
                Addressables.Release(handle);
                return size == 0;
            }

            NLogger.LogError($"[AddressablesRemote] Failed to get download size for key: '{key}'.");
            if (handle.IsValid()) Addressables.Release(handle);
            return false;
        }

        public static bool IsDownloadingAssets(string key)
        {
            return _activeDownloads.ContainsKey(key);
        }

        public static async UniTask<bool> DownloadAssets(
            string key,
            Action<long, long, float> onProgress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                NLogger.LogError("[AddressablesRemote] Invalid key provided to DownloadAssets.");
                return false;
            }

            if (!AddressablesLoader.IsInitialized)
            {
                await AddressablesLoader.Initialize();
            }

            if (await IsAssetsDownloaded(key))
            {
                onProgress?.Invoke(0, 0, 1f);
                return true;
            }

            if (_activeDownloads.TryGetValue(key, out var existingEntry))
            {
                if (onProgress != null)
                {
                    existingEntry.ProgressCallback += onProgress;
                }

                try
                {
                    return await existingEntry.Tcs.Task.AttachExternalCancellation(cancellationToken);
                }
                finally
                {
                    if (onProgress != null)
                    {
                        existingEntry.ProgressCallback -= onProgress;
                    }
                }
            }

            var entry = new DownloadEntry
            {
                Tcs = new UniTaskCompletionSource<bool>(),
                InternalCts = new CancellationTokenSource()
            };

            if (onProgress != null)
            {
                entry.ProgressCallback += onProgress;
            }

            var handle = Addressables.DownloadDependenciesAsync(key, false);
            entry.Handle = handle;
            _activeDownloads.Add(key, entry);

            MonitorDownloadAsync(key, entry, handle).Forget();

            try
            {
                return await entry.Tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                if (onProgress != null)
                {
                    entry.ProgressCallback -= onProgress;
                }
            }
        }

        private static async UniTaskVoid MonitorDownloadAsync(string key, DownloadEntry entry, AsyncOperationHandle handle)
        {
            var lastReportedPercent = -1f;

            try
            {
                while (!handle.IsDone && !entry.InternalCts.IsCancellationRequested)
                {
                    var status = handle.GetDownloadStatus();
                    if (status.Percent > lastReportedPercent + 0.01f || status.Percent >= 1f)
                    {
                        lastReportedPercent = status.Percent;
                        entry.ProgressCallback?.Invoke(status.DownloadedBytes, status.TotalBytes, status.Percent);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, entry.InternalCts.Token);
                }

                if (entry.InternalCts.IsCancellationRequested)
                {
                    _activeDownloads.Remove(key);
                    if (handle.IsValid()) Addressables.Release(handle);
                    entry.Tcs.TrySetResult(false);
                    return;
                }

                await handle.ToUniTask();

                bool succeeded = handle.Status == AsyncOperationStatus.Succeeded;
                if (succeeded)
                {
                    entry.ProgressCallback?.Invoke(1, 1, 1f);
                    _activeDownloads.Remove(key);
                    if (handle.IsValid()) Addressables.Release(handle);
                    entry.Tcs.TrySetResult(true);
                }
                else
                {
                    NLogger.LogError($"[AddressablesRemote] Failed to download dependencies for key: '{key}'.");
                    _activeDownloads.Remove(key);
                    if (handle.IsValid()) Addressables.Release(handle);
                    entry.Tcs.TrySetResult(false);
                }
            }
            catch (OperationCanceledException)
            {
                _activeDownloads.Remove(key);
                if (handle.IsValid()) Addressables.Release(handle);
                entry.Tcs.TrySetResult(false);
            }
            catch (Exception ex)
            {
                NLogger.LogError($"[AddressablesRemote] Exception during download for key '{key}': {ex.Message}");
                _activeDownloads.Remove(key);
                if (handle.IsValid()) Addressables.Release(handle);
                entry.Tcs.TrySetResult(false);
            }
        }

        public static bool StopDownloadAssets(string key)
        {
            if (_activeDownloads.TryGetValue(key, out var entry))
            {
                entry.InternalCts.Cancel();
                return true;
            }

            return false;
        }

        public static async UniTask<bool> ClearDownloadedAssetsOnDisk(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                NLogger.LogError("[AddressablesRemote] Invalid key provided to ClearDownloadedAssetsOnDisk.");
                return false;
            }

            if (!await IsAssetsDownloaded(key))
            {
                return false;
            }

            var handle = Addressables.ClearDependencyCacheAsync(key, false);
            await handle.ToUniTask();

            bool succeeded = handle.Status == AsyncOperationStatus.Succeeded;
            if (succeeded)
            {
                NLogger.Log($"[AddressablesRemote] Cleared downloaded asset with key: '{key}'.");
            }
            else
            {
                NLogger.LogError($"[AddressablesRemote] Failed to clear downloaded asset with key: '{key}'.");
            }

            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return succeeded;
        }

        public static async UniTask<bool> CleanBundleCache()
        {
            var handle = Addressables.CleanBundleCache();
            await handle.ToUniTask();

            bool succeeded = handle.Status == AsyncOperationStatus.Succeeded;
            if (succeeded)
            {
                NLogger.Log("[AddressablesRemote] Cleaned unused bundles cache.");
            }
            else
            {
                NLogger.LogError("[AddressablesRemote] Failed to clean unused bundles cache.");
            }

            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return succeeded;
        }
    }
}
#endif
