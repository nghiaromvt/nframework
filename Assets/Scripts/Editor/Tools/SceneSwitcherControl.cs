using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NFramework.Editors
{
    public class SceneSwitcherControl
    {
        private const string PREF_LAST_OPENED_SCENE = "Game.LastOpenedScene";
        private const string PREF_PLAYED_USING_RUN_UTILS = "Game.PlayedUsingRunUtils";

        private static Dictionary<string, string> _scenePathStorage = new Dictionary<string, string>();
        private static string[] _sceneTitles = Array.Empty<string>();
        private static bool _aboutToRun = false;

        static SceneSwitcherControl() => EditorApplication.playModeStateChanged += LoadLastOpenedScene;

        private static void LoadLastOpenedScene(PlayModeStateChange _modeStateChange)
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
            {
                // changed to playing or compiling
                // no need to do anything
                return;
            }

            if (!EditorPrefs.GetBool(PREF_PLAYED_USING_RUN_UTILS))
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
            // so we load the last opened scene
            string lastScene = EditorPrefs.GetString(PREF_LAST_OPENED_SCENE);
            if (!string.IsNullOrEmpty(lastScene))
                EditorSceneManager.OpenScene(lastScene);

            EditorPrefs.SetBool(PREF_PLAYED_USING_RUN_UTILS, false); // reset flag
        }

        public static void PlayFirstScene() => PlayGameProcess();

        private static void PlayGameProcess()
        {
            SceneSetup[] setups = EditorSceneManager.GetSceneManagerSetup();
            if (setups.Length > 0)
                EditorPrefs.SetString(PREF_LAST_OPENED_SCENE, setups[0].path);

            EditorPrefs.SetBool(PREF_PLAYED_USING_RUN_UTILS, true);
            _aboutToRun = true;

            EditorBuildSettingsScene firstScene =
                EditorBuildSettings.scenes.FirstOrDefault(_scene => _scene.enabled == true);

            OpenSceneWithSaveConfirm(firstScene.path);

            EditorApplication.isPlaying = true;
        }

        private static void OpenSceneWithSaveConfirm(string scenePath)
        {
            // Refresh first to cause compilation and include new assets
            AssetDatabase.Refresh();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(scenePath);
        }

        public static string[] GetActiveScenesInBuildSetting()
        {
            // Refresh scene data
            RefreshSceneData(ref _scenePathStorage);

            // Refresh scene titles
            refreshSceneTitles(ref _sceneTitles, _scenePathStorage);

            return _sceneTitles;
        }

        private static void RefreshSceneData(ref Dictionary<string, string> scenePathStorage)
        {
            // Clear dictionary
            scenePathStorage.Clear();

            // Apply new data to dictionary
            int sceneInBuildCount = EditorBuildSettings.scenes.Length;
            for (int i = 0; i < sceneInBuildCount; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

                if (scene.enabled == true)
                {
                    string scenePath = scene.path;
                    string key = ExtractSceneName(scenePath);
                    scenePathStorage.Add(key, scenePath);
                }
            }
        }

        private static string ExtractSceneName(string _fullPathScene)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(_fullPathScene);
            return sceneName;
        }

        private static void refreshSceneTitles(ref string[] titles, Dictionary<string, string> sceneData)
        {
            // Refresh scene title
            int activeSceneLength = sceneData.Count;
            if (titles.Length != activeSceneLength)
                titles = new string[activeSceneLength];

            int index = 0;
            foreach (KeyValuePair<string, string> pair in sceneData)
            {
                titles[index++] = pair.Key;
            }
        }

        public static void OpenScene(string scene)
        {
            if (_scenePathStorage.TryGetValue(scene, out string scenePath))
                OpenSceneWithSaveConfirm(scenePath);
        }
    }
}