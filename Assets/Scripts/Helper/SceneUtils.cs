using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public static class SceneUtils
    {
        public static IEnumerator CRLoadSceneAsync(int sceneBuildIndex, bool isAdditive = false, bool setActive = false, Action onLoadSceneCompleted = null)
        {
            yield return SceneManager.LoadSceneAsync(sceneBuildIndex, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            if (setActive)
            {
                yield return new WaitForEndOfFrame();
                var curScene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
                SceneManager.SetActiveScene(curScene);
            }

            onLoadSceneCompleted?.Invoke();
        }

        public static IEnumerator CRLoadSceneAsync(string sceneName, bool isAdditive = false, bool setActive = false, Action onLoadSceneCompleted = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError($"Invalid sceneName: {sceneName}");
                yield break;
            }

            yield return SceneManager.LoadSceneAsync(sceneName, isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            if (setActive)
            {
                yield return new WaitForEndOfFrame();
                var curScene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(curScene);
            }

            onLoadSceneCompleted?.Invoke();
        }
    }
}
