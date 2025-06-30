#if ADDRESSABLES
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NFramework
{
    public enum AddressableOperationStatus { None, Operating, Success, Failed, Error, Released }
    
    public class AddressablesManager : SingletonMono<AddressablesManager>
    {
        [SerializeField] private bool _initializeOnAwake = true;
        [SerializeField] private bool _isLog = true;
        
        private static readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableAssetLoaderDict = new();
        private static readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableAssetsLoaderDict = new();
        private static readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableSceneLoaderDict = new();
        private static readonly Dictionary<string, AddressableAssetDownloader> _curAddressableAssetDownloaderDict = new();
        
        public static bool IsInitialized { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (_initializeOnAwake) Initialize().Forget();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ReleaseAll();
        }

        public static async UniTask Initialize()
        {
            if (IsInitialized) return;

            var handle = Addressables.InitializeAsync(false);
            await handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                IsInitialized = true;
                Log("Initialized");
            }
            else
            {
                LogError("Failed to initialize");
            }

            Addressables.Release(handle);
        }

        /// <summary>
        /// Use it if only already ticked true on "Only update catalogs manually" in AddressableAssetSettings
        /// </summary>
        /// <param name="autoCleanBundleCache"> Clean unused bundles </param>
        public static async UniTask UpdateCatalogs(bool autoCleanBundleCache = true)
        {
            if (!IsInitialized) return;

            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            await checkHandle;

            if (checkHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var catalogIds = checkHandle.Result;
                if (catalogIds.Count > 0)
                {
                    Log($"Catalog ({catalogIds.Count}) need to be updated");
                    await Addressables.UpdateCatalogs(autoCleanBundleCache, checkHandle.Result);
                    Log("Catalog has been updated");
                }
            }
            else
            {
                LogError("Failed to check catalog updates!");
            }
            
            Addressables.Release(checkHandle);
        }

        #region Load

        /// <summary>
        /// Load specified asset
        /// </summary>
        /// <param name="key"> This can be the asset's address or the label. </param>
        /// <param name="onProgress"> Use that for purpose of display progress (downloadedBytes, totalBytes, downloadPercent). </param>
        /// <returns> Result of download </returns>
        public static async UniTask<T> LoadAsset<T>(string key, Action<float, float, float> onProgress = null) where T : Object
        {
            if (!IsInitialized) return null;
            if (key.IsNullOrEmpty()) return null;

            if (_cachedAddressableAssetLoaderDict.TryGetValue(key, out var cachedLoader))
            {
                if (onProgress != null)
                    cachedLoader.OnProgress += onProgress;
                
                await UniTask.WaitUntil(() => cachedLoader.Status != AddressableOperationStatus.Operating);
                
                if (onProgress != null)
                    cachedLoader.OnProgress -= onProgress;

                Log($"Succeed to load asset in cached with key: {key}");
                return ((AddressableAssetLoader<T>)cachedLoader).GetResult();
            }
            
            var loader = new AddressableAssetLoader<T>(key);
            _cachedAddressableAssetLoaderDict.Add(key, loader);
            
            await loader.Load();

            if (loader.Status == AddressableOperationStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableAssetLoaderDict.Remove(key);
            return null;
        }

        public static async UniTask<List<T>> LoadAssets<T>(string label, Action<float, float, float> onProgress = null) where T : Object
        {
            if (!IsInitialized) return null;
            if (label.IsNullOrEmpty()) return null;

            if (_cachedAddressableAssetsLoaderDict.TryGetValue(label, out var cachedLoader))
            {
                if (onProgress != null)
                    cachedLoader.OnProgress += onProgress;
                
                await UniTask.WaitUntil(() => cachedLoader.Status != AddressableOperationStatus.Operating);
                
                if (onProgress != null)
                    cachedLoader.OnProgress -= onProgress;

                Log($"Succeed to load assets in cached with label: {label}");
                return ((AddressableAssetsLoader<T>)cachedLoader).GetResult();
            }
            
            var loader = new AddressableAssetsLoader<T>(label);
            _cachedAddressableAssetsLoaderDict.Add(label, loader);
            
            await loader.Load();

            if (loader.Status == AddressableOperationStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableAssetsLoaderDict.Remove(label);
            return null;
        }

        public static async UniTask<SceneInstance> LoadScene(string key, LoadSceneMode loadMode = LoadSceneMode.Single,
            bool setActiveScene = true, bool activateOnLoad = true, Action<float, float, float> onProgress = null)
        {
            if (!IsInitialized) return default;
            if (key.IsNullOrEmpty()) return default;

            if (_cachedAddressableSceneLoaderDict.TryGetValue(key, out var cachedLoader))
            {
                if (onProgress != null)
                    cachedLoader.OnProgress += onProgress;
                
                await UniTask.WaitUntil(() => cachedLoader.Status != AddressableOperationStatus.Operating);
                
                if (onProgress != null)
                    cachedLoader.OnProgress -= onProgress;

                Log($"Succeed to load scene in cached with key: {key}");
                return ((AddressableSceneLoader)cachedLoader).GetResult();
            }
            
            var loader = new AddressableSceneLoader(key, loadMode, setActiveScene, activateOnLoad);
            _cachedAddressableSceneLoaderDict.Add(key, loader);
            
            await loader.Load();

            if (loader.Status == AddressableOperationStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableSceneLoaderDict.Remove(key);
            return default;
        }

        #endregion

        #region Unload/Release

        public static bool ReleaseAsset(string key)
        {
            if (!IsInitialized) return false;
            if (key.IsNullOrEmpty()) return false;

            if (_cachedAddressableAssetLoaderDict.TryGetValue(key, out var cachedLoader))
            {
                cachedLoader.Release();
                _cachedAddressableAssetLoaderDict.Remove(key);
                return true;
            }
            
            return false;
        }
        
        public static bool ReleaseAssets(string label)
        {
            if (!IsInitialized) return false;
            if (label.IsNullOrEmpty()) return false;

            if (_cachedAddressableAssetsLoaderDict.TryGetValue(label, out var cachedLoader))
            {
                cachedLoader.Release();
                _cachedAddressableAssetsLoaderDict.Remove(label);
                return true;
            }
            
            return false;
        }

        public static async UniTask<bool> UnloadScene(string key)
        {
            if (!IsInitialized) return false;
            if (key.IsNullOrEmpty()) return false;

            if (_cachedAddressableSceneLoaderDict.TryGetValue(key, out var cachedLoader))
            {
                Log($"Unload scene with key:{key}");
                await ((AddressableSceneLoader)cachedLoader).Unload();
                _cachedAddressableSceneLoaderDict.Remove(key);
                return true;
            }
            
            return false;
        }
        
        private void ReleaseAll()
        {
            _cachedAddressableAssetLoaderDict.Values.ForEach(loader => loader.Release());
            _cachedAddressableAssetsLoaderDict.Values.ForEach(loader => loader.Release());
            _cachedAddressableSceneLoaderDict.Values.ForEach(loader => loader.Release());
            _cachedAddressableAssetLoaderDict.Clear();
            _cachedAddressableAssetsLoaderDict.Clear();
            _cachedAddressableSceneLoaderDict.Clear();
        }

        #endregion

        #region Download

        /// <summary>
        /// Download specified assets from remote.
        /// </summary>
        /// <param name="key"> This can be the asset's address or the label. </param>
        /// <param name="onProgress"> Use that for purpose of display progress (downloadedBytes, totalBytes, downloadPercent). </param>
        /// <returns> Result of download </returns>
        public static async UniTask<bool> DownloadAssets(string key, Action<float, float, float> onProgress = null)
        {
            if (!IsInitialized || string.IsNullOrEmpty(key))
                return false;

            if (await IsAssetsDownloaded(key))
                return true;

            if (IsDownloadingAssets(key))
            {
                var downloader = _curAddressableAssetDownloaderDict[key];
                
                if (onProgress != null)
                    downloader.OnProgress += onProgress;
                
                await UniTask.WaitUntil(() => downloader.Status != AddressableOperationStatus.Operating);
                
                if (onProgress != null)
                    downloader.OnProgress -= onProgress;
                
                return downloader.Status == AddressableOperationStatus.Success;
            }
            else
            {
                var downloader = new AddressableAssetDownloader(key);
                _curAddressableAssetDownloaderDict.Add(key,downloader);

                if (onProgress != null)
                    downloader.OnProgress += onProgress;
                
                await downloader.Download();
                
                if (onProgress != null)
                    downloader.OnProgress -= onProgress;
                
                _curAddressableAssetDownloaderDict.Remove(key);
                return downloader.Status == AddressableOperationStatus.Success;
            }
        }

        public static bool StopDownloadAssets(string key)
        {
            if (_curAddressableAssetDownloaderDict.TryGetValue(key, out var downloader))
            {
                downloader.Release();
                _curAddressableAssetDownloaderDict.Remove(key);
                return true;
            }

            return false;
        }
        
        /// <param name="key"> This can be the asset's address or the label. </param>
        public static async UniTask<bool> ClearDownloadedAssetsOnDisk(string key)
        {
            if (key.IsNullOrEmpty()) return false;
            if (!await IsAssetsDownloaded(key)) return false;
            
            var handle = Addressables.ClearDependencyCacheAsync(key, false);
            while (!handle.IsDone) await UniTask.Yield();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Log($"Succeed to clear downloaded asset with key: {key}");
                return true;
            }
      
            Log($"Failed to clear downloaded asset with key: {key}");
            return false;
        }
        
        public static async UniTask<bool> CleanUnusedBundleCache()
        {
            var handle = Addressables.CleanBundleCache();
            await handle.ToUniTask();
            Addressables.Release(handle);

            bool success = handle.Status == AsyncOperationStatus.Succeeded;
            Log(success ? "Cleaned unused bundles." : "Failed to clean unused bundles.");
            return success;
        }
        
        public static bool IsDownloadingAssets(string key) => _curAddressableAssetDownloaderDict.ContainsKey(key);

        public static async UniTask<bool> IsAssetsDownloaded(string key)
        {
            var handle = Addressables.GetDownloadSizeAsync(key);
            await handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                long size = handle.Result;
                Addressables.Release(handle);
                return size == 0;
            }
            
            LogError($"Failed to get download size for address:{key}");
            Addressables.Release(handle);
            return false;
        }

        #endregion

        #region Log

        public static void Log(string message)
        {
            if (I._isLog) NLogger.Log(message, I, Color.green);
        }

        public static void LogError(string message)
        {
            if (I._isLog) NLogger.LogError(message, I);
        }

        #endregion

        #region Others

        public static bool IsSceneLoadByAddressables(string key) => _cachedAddressableSceneLoaderDict.ContainsKey(key);

        public static bool IsAssetLoadedByAddressables(string key) => _cachedAddressableAssetLoaderDict.ContainsKey(key);
        
        #endregion
    }
}
#endif