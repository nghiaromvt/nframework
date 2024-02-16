using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NFramework
{
    public static class SceneLoader
    {
        public static IEnumerator CRLoadScene(string sceneName, bool isAdditive = false, bool setActive = false)
        {
            if (sceneName == null)
                yield return null;

            var isLoaded = SceneManager.LoadSceneAsync(sceneName, isAdditive ? LoadSceneMode.Additive : 0);

            while (!isLoaded.isDone)
            {
                yield return null;
            }

            if (setActive)
            {
                yield return new WaitForEndOfFrame();
                var curScene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(curScene);
            }
        }

        public static void UnloadScene(string sceneName) => SceneManager.UnloadSceneAsync(sceneName);
    }
}
