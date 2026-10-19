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

        public static async UniTask Load(
            string sceneName,
            bool isAdditive = false,
            bool setActive = false,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            var asyncOperation = SceneManager.LoadSceneAsync(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            if (asyncOperation == null)
            {
                throw new InvalidOperationException($"Failed to load scene '{sceneName}'. Scene may not exist or is not added to Build Settings.");
            }

            await asyncOperation.ToUniTask(progress: progress, cancellationToken: cancellationToken);

#if ADDRESSABLES
            if (!isAdditive)
            {
                CleanUnloadedAddressableScenes();
            }
#endif

            if (setActive)
            {
                SetActive(sceneName);
            }
        }

        public static async UniTask Unload(
            string sceneName,
            string nextActiveSceneName = null,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException($"Cannot unload scene '{sceneName}': Scene is not valid or not loaded.");
            }

            var asyncOperation = SceneManager.UnloadSceneAsync(scene);
            if (asyncOperation == null)
            {
                throw new InvalidOperationException($"Failed to start unloading scene '{sceneName}'.");
            }

            await asyncOperation.ToUniTask(progress: progress, cancellationToken: cancellationToken);

            if (!string.IsNullOrEmpty(nextActiveSceneName))
            {
                SetActive(nextActiveSceneName);
            }
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
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            if (!AddressablesLoader.IsInitialized)
            {
                await AddressablesLoader.Initialize();
            }

            if (_addressableScenesByAddress.ContainsKey(address))
            {
                throw new InvalidOperationException($"Addressable scene '{address}' is already loaded.");
            }

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
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }

                throw new InvalidOperationException($"Failed to load Addressables scene: '{address}'.");
            }
        }

        public static async UniTask UnloadAddressables(
            string address,
            string nextActiveSceneName = null,
            IProgress<float> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            if (!_addressableScenesByAddress.TryGetValue(address, out var entry))
            {
                throw new InvalidOperationException($"Addressable scene '{address}' is not loaded or tracked.");
            }

            _addressableScenesByAddress.Remove(address);
            _addressableScenesByScene.Remove(entry.Scene);

            if (entry.Handle.IsValid() && entry.Scene.IsValid() && entry.Scene.isLoaded)
            {
                var unloadHandle = Addressables.UnloadSceneAsync(entry.Handle, false);
                while (!unloadHandle.IsDone)
                {
                    progress?.Report(unloadHandle.PercentComplete);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                }

                await unloadHandle.ToUniTask().AttachExternalCancellation(cancellationToken);
                progress?.Report(1f);

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
                {
                    continue;
                }

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
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException("Scene name cannot be null or empty.", nameof(sceneName));
            }

            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException($"Cannot set active scene '{sceneName}': Scene is not valid or not loaded.");
            }

            SceneManager.SetActiveScene(scene);
        }
    }
}