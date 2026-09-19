using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace NFramework
{
    public static class SceneLoader
    {
#if ADDRESSABLES
        private class AddressableSceneEntry
        {
            public string Address;
            public Scene Scene;
            public AsyncOperationHandle<SceneInstance> Handle;
        }

        private static readonly Dictionary<string, AddressableSceneEntry> _addressableScenesByAddress = new();
        private static readonly Dictionary<Scene, AddressableSceneEntry> _addressableScenesByScene = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _addressableScenesByAddress.Clear();
            _addressableScenesByScene.Clear();
        }
#endif

        public static async UniTask Load(string sceneName, bool isAdditive = false, bool setActive = false, Action<float> onProgress = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                NLogger.LogError($"[SceneLoader] Invalid sceneName: {sceneName}");
                return;
            }

            NLogger.Log($"[SceneLoader] Start load scene: {sceneName}");
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            while (!asyncOperation.isDone)
            {
                onProgress?.Invoke(asyncOperation.progress);
                await UniTask.Yield(Application.exitCancellationToken);
            }

#if ADDRESSABLES
            if (!isAdditive)
            {
                // In Single mode, Unity unloads all other scenes including Addressable scenes
                CleanUnloadedAddressableScenes();
            }
#endif
            
            if (setActive)
                SetActive(sceneName);
        }

        public static async UniTask Unload(string sceneName, string nextActiveSceneName = null, Action<float> onProgress = null)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid())
            {
                NLogger.LogError($"[SceneLoader] Invalid scene: {sceneName}");
                return;
            }

            NLogger.Log($"[SceneLoader] Start unload scene {sceneName}");
            var asyncOperation = SceneManager.UnloadSceneAsync(scene);

            while (!asyncOperation.isDone)
            {
                onProgress?.Invoke(asyncOperation.progress);
                await UniTask.Yield(Application.exitCancellationToken);
            }
            
            if (!string.IsNullOrEmpty(nextActiveSceneName))
                SetActive(nextActiveSceneName);
        }
        
#if ADDRESSABLES
        public static async UniTask LoadAddressables(
            string address,
            bool isAdditive = false,
            bool setActive = false,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(address))
            {
                NLogger.LogError($"[SceneLoader] Invalid address: {address}");
                return;
            }

            if (!AddressablesLoader.IsInitialized)
            {
                await AddressablesLoader.Initialize();
            }

            if (_addressableScenesByAddress.ContainsKey(address))
            {
                NLogger.LogWarning($"[SceneLoader] Addressable scene '{address}' is already loaded.");
                return;
            }

            NLogger.Log($"[SceneLoader] Start load Addressables scene: {address}");
            var loadMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var handle = Addressables.LoadSceneAsync(address, loadMode, true);

            while (!handle.IsDone)
            {
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            await handle.ToUniTask().AttachExternalCancellation(cancellationToken);
            progress?.Report(1f);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var entry = new AddressableSceneEntry
                {
                    Address = address,
                    Scene = handle.Result.Scene,
                    Handle = handle
                };

                if (!isAdditive)
                {
                    CleanUnloadedAddressableScenes();
                }

                _addressableScenesByAddress[address] = entry;
                _addressableScenesByScene[entry.Scene] = entry;

                if (setActive)
                {
                    SceneManager.SetActiveScene(entry.Scene);
                }
            }
            else
            {
                NLogger.LogError($"[SceneLoader] Failed to load Addressables scene: {address}");
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        public static async UniTask UnloadAddressables(string address, string nextActiveSceneName = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                NLogger.LogError($"[SceneLoader] Invalid address: {address}");
                return;
            }

            if (!_addressableScenesByAddress.TryGetValue(address, out var entry))
            {
                NLogger.LogError($"[SceneLoader] Addressable scene '{address}' is not loaded or tracked.");
                return;
            }

            NLogger.Log($"[SceneLoader] Start unload Addressables scene: {address}");
            _addressableScenesByAddress.Remove(address);
            _addressableScenesByScene.Remove(entry.Scene);

            if (entry.Handle.IsValid() && entry.Scene.IsValid() && entry.Scene.isLoaded)
            {
                var unloadHandle = Addressables.UnloadSceneAsync(entry.Handle, false);
                await unloadHandle.ToUniTask();
                if (unloadHandle.IsValid())
                {
                    Addressables.Release(unloadHandle);
                }
            }

            if (!string.IsNullOrEmpty(nextActiveSceneName))
            {
                SetActive(nextActiveSceneName);
            }
        }

        private static void CleanUnloadedAddressableScenes()
        {
            var staleList = new List<AddressableSceneEntry>();
            foreach (var entry in _addressableScenesByAddress.Values)
            {
                if (!entry.Scene.IsValid() || !entry.Scene.isLoaded)
                {
                    staleList.Add(entry);
                }
            }

            foreach (var entry in staleList)
            {
                _addressableScenesByAddress.Remove(entry.Address);
                _addressableScenesByScene.Remove(entry.Scene);
                if (entry.Handle.IsValid())
                {
                    Addressables.Release(entry.Handle);
                }
            }
        }

        public static bool IsSceneLoadedByAddressables(string address) => _addressableScenesByAddress.ContainsKey(address);
        public static bool IsSceneLoadedByAddressables(Scene scene) => _addressableScenesByScene.ContainsKey(scene);
#endif

        public static Scene[] GetAllLoaded()
        {
            var countLoaded = SceneManager.sceneCount;
            var loadedScenes = new Scene[countLoaded];

            for (var i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            return loadedScenes;
        }

        public static async UniTask UnloadAll(params string[] exceptSceneNames)
        {
            foreach (var scene in GetAllLoaded())
            {
                if (exceptSceneNames != null && exceptSceneNames.Contains(scene.name))
                    continue;

#if ADDRESSABLES
                if (_addressableScenesByScene.TryGetValue(scene, out var entry))
                {
                    await UnloadAddressables(entry.Address);
                    continue;
                }
#endif
                await Unload(scene.name);
            }
        }

        public static void SetActive(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid())
            {
                NLogger.LogError($"Invalid scene: {sceneName}");
                return;
            }

            SceneManager.SetActiveScene(scene);
        }
    }
}