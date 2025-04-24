using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NFramework.Editor
{
    public static class PlayGameMenuItem
    {
        [MenuItem("NFramework/Play Game ^SPACE", priority = 0)]
        public static void PlayGame() => SceneSwitcherControl.PlayGame();
    }

    public static class SceneSwitcherControl
    {
        private static string LastOpenedScenesPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("LastOpenedScenes");
        private static string PlayedUsingRunUtilsPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("PlayedUsingRunUtils");

        private static Dictionary<string, string> _scenePathStorage = new();
        private static string[] _sceneTitles = Array.Empty<string>();
        private static bool _aboutToRun;

        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= LoadLastOpenedScene;
            EditorApplication.playModeStateChanged += LoadLastOpenedScene;
        }

        public static void PlayGame()
        {
            SaveOpenedScenes();
            EditorBuildSettingsScene firstScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled);
            var isAccept = OpenSceneWithSaveConfirm(firstScene.path);
            if (isAccept)
            {
                EditorPrefs.SetBool(PlayedUsingRunUtilsPrefsKey, true);
                _aboutToRun = true;
                EditorApplication.isPlaying = true;
            }
        }

        public static string[] GetActiveScenesInBuildSetting()
        {
            RefreshSceneData(ref _scenePathStorage);
            RefreshSceneTitles(ref _sceneTitles, _scenePathStorage);
            return _sceneTitles;
        }

        public static void OpenScene(string scene)
        {
            if (_scenePathStorage.TryGetValue(scene, out string scenePath))
                OpenSceneWithSaveConfirm(scenePath);
        }

        private static void SaveOpenedScenes()
        {
            SceneSetup[] setups = EditorSceneManager.GetSceneManagerSetup();
            if (setups.Length == 0)
                return;

            setups = setups.OrderByDescending(x => x.isLoaded).ThenByDescending(x => x.isActive).ToArray();
            // Use '?' to add more info of scene and '|' to separate scenes
            var str = "";
            for (int i = 0; i < setups.Length; i++)
            {
                var sceneSetup = setups[i];
                str += sceneSetup.path;
                str += $"?{sceneSetup.isLoaded}";
                if (setups.Length > 1 && i < setups.Length - 1)
                {
                    str += "|";
                }
            }

            EditorPrefs.SetString(LastOpenedScenesPrefsKey, str);
        }

        private static void LoadLastOpenedScene(PlayModeStateChange modeStateChange)
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                // changed to playing or compiling
                // no need to do anything
                return;
            }

            if (!EditorPrefs.GetBool(PlayedUsingRunUtilsPrefsKey))
            {
                // this means that normal play mode might have been used
                return;
            }

            // We added this check because this method is still invoked while EditorApplication.isPlaying is false
            // We only load the last opened scene when the aboutToRun flag is "consumed"
            if (_aboutToRun)
            {
                _aboutToRun = false;
                return;
            }

            // at this point, the scene has stopped playing
            // so we load the last opened scenes
            var lastOpenedScenes = EditorPrefs.GetString(LastOpenedScenesPrefsKey);
            if (!string.IsNullOrEmpty(lastOpenedScenes))
            {
                var scenes = lastOpenedScenes.Split('|');
                for (int i = 0; i < scenes.Length; i++)
                {
                    var sceneSplits = scenes[i].Split('?');
                    var scenePath = sceneSplits[0];
                    var isLoaded = bool.Parse(sceneSplits[1]);

                    EditorSceneManager.OpenScene(scenePath, i == 0 ? OpenSceneMode.Single :
                        isLoaded ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading);
                }
            }

            EditorPrefs.SetBool(PlayedUsingRunUtilsPrefsKey, false); // reset flag
        }

        private static bool OpenSceneWithSaveConfirm(string scenePath)
        {
            // Refresh first to cause compilation and include new assets
            AssetDatabase.Refresh();

            var isAccept = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            if (isAccept)
                EditorSceneManager.OpenScene(scenePath);

            return isAccept;
        }

        private static void RefreshSceneData(ref Dictionary<string, string> scenePathStorage)
        {
            scenePathStorage.Clear();

            // Apply new data to dictionary
            int sceneInBuildCount = EditorBuildSettings.scenes.Length;
            for (int i = 0; i < sceneInBuildCount; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

                if (scene.enabled)
                {
                    string scenePath = scene.path;
                    string key = ExtractSceneName(scenePath);
                    scenePathStorage.Add(key, scenePath);
                }
            }
        }

        private static string ExtractSceneName(string fullPathScene)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(fullPathScene);
            return sceneName;
        }

        private static void RefreshSceneTitles(ref string[] titles, Dictionary<string, string> sceneData)
        {
            int activeSceneLength = sceneData.Count;
            if (titles.Length != activeSceneLength)
                titles = new string[activeSceneLength];

            int index = 0;
            foreach (KeyValuePair<string, string> pair in sceneData)
            {
                titles[index++] = pair.Key;
            }
        }
    }
}
