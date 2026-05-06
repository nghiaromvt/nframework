#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NFramework
{
    public class NFrameworkConfigSO : ScriptableObject
    {
        public string scriptDefineNamespace;
        
        [Header("UI")]
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string uiScriptDefineSavePath;
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string uiViewsFolderPath;
        
        [Header("Sound")]
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string soundScriptDefineSavePath;
        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string soundGroupFolderPath;
        
        private static NFrameworkConfigSO _instance;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(scriptDefineNamespace))
                scriptDefineNamespace = EditorSettings.projectGenerationRootNamespace;
        }

        public static NFrameworkConfigSO GetConfig()
        {
            if (_instance)
                return _instance;
            
            var config = FileHelper.LoadFirstAssetWithName<NFrameworkConfigSO>("NFrameworkConfigSO");

            if (config == null)
            {
                config = CreateInstance<NFrameworkConfigSO>();
                config.OnValidate();

                AssetDatabase.CreateAsset(config, "Assets/NFrameworkConfigSO.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"Created NFrameworkConfigSO");
            }
            
            _instance = config;

            return config;
        }
    }
}
#endif