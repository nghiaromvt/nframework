using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public static class SceneLoader
    {
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
                await UniTask.Yield();
            }
            
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
                await UniTask.Yield();
            }
            
            if (!string.IsNullOrEmpty(nextActiveSceneName))
                SetActive(nextActiveSceneName);
        }
        
#if ADDRESSABLES
        public static async UniTask LoadAddressables(string sceneName, bool isAdditive = false, bool setActive = false)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                NLogger.LogError($"[SceneLoader] Invalid sceneName: {sceneName}");
                return;
            }

            NLogger.Log($"[SceneLoader] Start load scene: {sceneName}");
            await AddressablesManager.LoadScene(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single, setActive);
        }

        public static async UniTask UnloadAddressables(string sceneName, bool showLoading = false, string nextActiveSceneName = null)
        {
            var scene = SceneManager.GetSceneByName(sceneName);

            if (!scene.IsValid())
            {
                NLogger.LogError($"[SceneLoader] Invalid scene: {sceneName}");
                return;
            }

            NLogger.Log($"[SceneLoader] Start unload scene {sceneName}");
            await AddressablesManager.UnloadScene(sceneName);

            if (!string.IsNullOrEmpty(nextActiveSceneName))
                SetActive(nextActiveSceneName);
        }
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
                if (!exceptSceneNames.Contains(scene.name))
                {
#if ADDRESSABLES
                    if (AddressablesManager.IsSceneLoadByAddressables(scene.name))
                        await UnloadAddressables(scene.name);
                    else
#endif
                        await Unload(scene.name);
                }
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