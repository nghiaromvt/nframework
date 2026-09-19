using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using NFramework.UI;

namespace NFramework.UI.Editor
{
    public class UIConfigSO : ScriptableObject
    {
        public string ScriptDefineNamespace;

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string ScriptDefineSavePath;

        private static UIConfigSO _instance;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ScriptDefineNamespace))
            {
                ScriptDefineNamespace = EditorSettings.projectGenerationRootNamespace;
            }
        }

        public static UIConfigSO GetConfig()
        {
            if (_instance)
            {
                return _instance;
            }

            var configs = FileHelper.LoadAssetsWithType<UIConfigSO>();
            UIConfigSO selectedConfig = null;

            if (configs.Count > 0)
            {
                selectedConfig = configs.Find(c =>
                {
                    var path = AssetDatabase.GetAssetPath(c);
                    return !path.Contains("/Sample/") && !c.name.Contains("Sample");
                });

                if (selectedConfig == null)
                {
                    selectedConfig = configs[0];
                }
            }

            if (selectedConfig == null)
            {
                selectedConfig = CreateInstance<UIConfigSO>();
                selectedConfig.OnValidate();

                AssetDatabase.CreateAsset(selectedConfig, "Assets/UIConfigSO.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Created UIConfigSO");
            }

            _instance = selectedConfig;
            return selectedConfig;
        }
    }
}
