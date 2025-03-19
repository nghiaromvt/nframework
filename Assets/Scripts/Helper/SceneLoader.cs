using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

#if ADDRESSABLE
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace NFramework
{
    public static class SceneLoader
    {
#if ADDRESSABLE
        private static readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenes = new();
#endif

        public static async void LoadSceneAsync(string key, bool isAdditive = true, bool activeOnLoad = true, Action onLoadComplete = null)
        {
#if ADDRESSABLE
            if (_loadedScenes.ContainsKey(key))
            {
                Debug.LogWarning($"Scene {key} is already loaded");
                return;
            }

            var loadSceneMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var handle = Addressables.LoadSceneAsync(key, loadSceneMode, activeOnLoad);

            while (!handle.IsDone)
            {
                await Task.Yield();
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Scene {key} loaded successfully");
                _loadedScenes[key] = handle;
                onLoadComplete?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to load scene {key}");
            }
#else
            var handle = SceneManager.LoadSceneAsync(key, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            while (!handle.isDone)
            {
                await Task.Yield();
            }

            if (activeOnLoad)
            {
                var curScene = SceneManager.GetSceneByName(key);
                SceneManager.SetActiveScene(curScene);
            }

            onLoadComplete?.Invoke();
#endif

        }

        public static async void UnloadSceneAsync(string key, Action onUnloadComplete = null)
        {
#if ADDRESSABLE
            if (!_loadedScenes.TryGetValue(key, out var handle))
            {
                Debug.LogWarning($"Scene {key} is not loaded or already unloaded.");
                return;
            }

            _loadedScenes.Remove(key);
            var unloadHandle = Addressables.UnloadSceneAsync(handle);

            unloadHandle.Completed += _ =>
            {
                unloadHandle.Release();
                Resources.UnloadUnusedAssets();
                onUnloadComplete?.Invoke();
            };
#else
            var scene = SceneManager.GetSceneByName(key);
            if (scene.IsValid())
            {
                var handle = SceneManager.UnloadSceneAsync(scene);
                while (!handle.isDone)
                {
                    await Task.Yield();
                }
            }
            onUnloadComplete?.Invoke();
#endif
        }
    }
}
