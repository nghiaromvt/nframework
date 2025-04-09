using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace NFramework
{
    public class AddressablesManager : SingletonMono<AddressablesManager>
    {
        [SerializeField] private bool _initializeOnAwake;
        
        private readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableAssetLoaderDict = new();
        private readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableAssetsLoaderDict = new();
        private readonly Dictionary<string, BaseAddressableLoader> _cachedAddressableSceneLoaderDict = new();
        
        public bool IsInitialized { get; private set; }

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

        public async UniTask Initialize()
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
        public async UniTask UpdateCatalogs(bool autoCleanBundleCache = true)
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

        // public async UniTask ClearDependencyCache(bool releaseResourcesFirst = true, params string[] keys)
        // {
        //     if (keys.IsNullOrEmpty()) return;
        //     
        //     if 
        //     
        //     Addressables.ClearDependencyCacheAsync(keys.AsEnumerable(), true);
        //
        // }

        #region Load

        public async UniTask<T> LoadAsset<T>(string address) where T : Object
        {
            if (!IsInitialized) return null;
            if (address.IsNullOrEmpty()) return null;

            if (_cachedAddressableAssetLoaderDict.TryGetValue(address, out var cachedLoader))
            {
                if (cachedLoader.Status == EAddressableLoaderStatus.Loading)
                    await UniTask.WaitUntil(() => cachedLoader.Status != EAddressableLoaderStatus.Loading);

                Log($"Succeed to load asset in cached with address:{address}");
                return ((AddressableAssetLoader<T>)cachedLoader).GetResult();
            }
            
            var loader = new AddressableAssetLoader<T>(address);
            _cachedAddressableAssetLoaderDict.Add(address, loader);
            
            await loader.Load();

            if (loader.Status == EAddressableLoaderStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableAssetLoaderDict.Remove(address);
            return null;
        }

        public async UniTask<List<T>> LoadAssets<T>(string label) where T : Object
        {
            if (!IsInitialized) return null;
            if (label.IsNullOrEmpty()) return null;

            if (_cachedAddressableAssetsLoaderDict.TryGetValue(label, out var cachedLoader))
            {
                if (cachedLoader.Status == EAddressableLoaderStatus.Loading)
                    await UniTask.WaitUntil(() => cachedLoader.Status != EAddressableLoaderStatus.Loading);

                Log($"Succeed to load assets in cached with label:{label}");
                return ((AddressableAssetsLoader<T>)cachedLoader).GetResult();
            }
            
            var loader = new AddressableAssetsLoader<T>(label);
            _cachedAddressableAssetsLoaderDict.Add(label, loader);
            
            await loader.Load();

            if (loader.Status == EAddressableLoaderStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableAssetsLoaderDict.Remove(label);
            return null;
        }

        public async UniTask<SceneInstance> LoadScene(string address, LoadSceneMode loadMode = LoadSceneMode.Single,
            bool setActiveScene = true, bool activateOnLoad = true)
        {
            if (!IsInitialized) return default;
            if (address.IsNullOrEmpty()) return default;

            if (_cachedAddressableSceneLoaderDict.TryGetValue(address, out var cachedLoader))
            {
                if (cachedLoader.Status == EAddressableLoaderStatus.Loading)
                    await UniTask.WaitUntil(() => cachedLoader.Status != EAddressableLoaderStatus.Loading);

                Log($"Succeed to load scene in cached with address:{address}");
                return ((AddressableSceneLoader)cachedLoader).GetResult();
            }
            
            var loader = new AddressableSceneLoader(address, loadMode, setActiveScene, activateOnLoad);
            _cachedAddressableSceneLoaderDict.Add(address, loader);
            
            await loader.Load();

            if (loader.Status == EAddressableLoaderStatus.Success)
                return loader.GetResult();
            
            _cachedAddressableSceneLoaderDict.Remove(address);
            return default;
        }

        #endregion

        #region Unload/Release

        public void ReleaseAsset(string address)
        {
            if (!IsInitialized) return;
            if (address.IsNullOrEmpty()) return;

            if (_cachedAddressableAssetLoaderDict.TryGetValue(address, out var cachedLoader))
            {
                Log($"Release asset with address:{address}");
                cachedLoader.Release();
                _cachedAddressableAssetLoaderDict.Remove(address);
            }
        }
        
        public void ReleaseAssets(string label)
        {
            if (!IsInitialized) return;
            if (label.IsNullOrEmpty()) return;

            if (_cachedAddressableAssetsLoaderDict.TryGetValue(label, out var cachedLoader))
            {
                Log($"Release assets with label:{label}");
                cachedLoader.Release();
                _cachedAddressableAssetsLoaderDict.Remove(label);
            }
        }

        public async UniTask UnloadScene(string address)
        {
            if (!IsInitialized) return;
            if (address.IsNullOrEmpty()) return;

            if (_cachedAddressableSceneLoaderDict.TryGetValue(address, out var cachedLoader))
            {
                Log($"Unload scene with address:{address}");
                await ((AddressableSceneLoader)cachedLoader).Unload();
                _cachedAddressableSceneLoaderDict.Remove(address);
            }
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
        
        [Conditional("ENABLE_ADDRESSABLES_LOG"), Conditional("UNITY_EDITOR")]
        public static void Log(string message) => NLogger.Log(message, I, Color.green);
        
        [Conditional("ENABLE_ADDRESSABLES_LOG"), Conditional("UNITY_EDITOR"), Conditional("ENABLE_ERROR_LOG")]
        public static void LogError(string message) => NLogger.Log(message, I);
    }
}
