using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NFramework.Editors
{
    public class SceneSwitcherControl
    {
        private static readonly string PREF_LAST_OPENED_SCENES = $"{Application.dataPath}.LastOpenedScenes";
        private static readonly string PREF_PLAYED_USING_RUN_UTILS = $"{Application.dataPath}.PlayedUsingRunUtils";

        private static Dictionary<string, string> _scenePathStorage = new Dictionary<string, string>();
        private static string[] _sceneTitles = Array.Empty<string>();
        private static bool _aboutToRun = false;

        static SceneSwitcherControl() => EditorApplication.playModeStateChanged += LoadLastOpenedScene;

        public static void PlayGame()
        {
            SaveOpenedScenes();
            EditorBuildSettingsScene firstScene = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled == true);
            var isAccept = OpenSceneWithSaveConfirm(firstScene.path);
            if (isAccept)
            {
                EditorPrefs.SetBool(PREF_PLAYED_USING_RUN_UTILS, true);
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
            // Use '?' to add more info of scene and '|' to seperate scenes
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

            EditorPrefs.SetString(PREF_LAST_OPENED_SCENES, str);
        }

        private static void LoadLastOpenedScene(PlayModeStateChange modeStateChange)
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
            // so we load the last opened scenes
            var lastOpenedScenes = EditorPrefs.GetString(PREF_LAST_OPENED_SCENES);
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

            EditorPrefs.SetBool(PREF_PLAYED_USING_RUN_UTILS, false); // reset flag
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