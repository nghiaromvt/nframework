using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NFramework.Editor
{
    public class SceneEditorWindow : OdinEditorWindow
    {
        [MenuItem("NFramework/Scene Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<SceneEditorWindow>();
            window.Init();
            window.Show();
        }

        [Serializable]
        public class SceneData
        {
            [ReadOnly, HorizontalGroup(0.2f), HideLabel] public string name;
            [ReadOnly, HorizontalGroup, HideLabel] public string path;
            
            [Button, HorizontalGroup(0.15f)]
            public void Open() => SceneSwitcherControl.OpenSceneWithSaveConfirm(path);

            [Button, HorizontalGroup(0.15f)]
            public void Add() => EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            [Button, HorizontalGroup(0.15f)]
            public void Locate()
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                Selection.activeObject = asset;
            }
        }
        
        [Searchable] public List<SceneData> activeInBuildSettingSceneDatas = new();
        [Searchable] public List<SceneData> allSceneDatas = new();

        [OnInspectorInit]
        public void Init() => Refresh();
        
        [Button]
        public void Refresh()
        {
            activeInBuildSettingSceneDatas = GetActiveScenesInBuildSetting();
            allSceneDatas = GetAllScenesInProject();
        }

        private List<SceneData> GetActiveScenesInBuildSetting()
        {
            var result = new List<SceneData>();
            int sceneInBuildCount = EditorBuildSettings.scenes.Length;
            for (int i = 0; i < sceneInBuildCount; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];

                if (scene.enabled)
                {
                    result.Add(new SceneData
                    {
                        name = System.IO.Path.GetFileNameWithoutExtension(scene.path),
                        path = scene.path,
                    });
                }
            }
            return result;
        }
        
        private List<SceneData> GetAllScenesInProject()
        {
            var result = new List<SceneData>();
            var scenePaths = FileHelper.GetAssetPaths("t:SceneAsset", "Assets");
            foreach (var path in scenePaths)
            {
                result.Add(new SceneData
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path),
                    path = path,
                });
            }
            return result;
        }
    }
}
