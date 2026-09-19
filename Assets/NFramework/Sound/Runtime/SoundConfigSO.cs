#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NFramework.Sound
{
    public class SoundConfigSO : ScriptableObject
    {
        public string scriptDefineNamespace;

        [FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        public string soundScriptDefineSavePath;

        private static SoundConfigSO _instance;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(scriptDefineNamespace))
                scriptDefineNamespace = EditorSettings.projectGenerationRootNamespace;
        }

        public static SoundConfigSO GetConfig()
        {
            if (_instance)
                return _instance;

            var config = FileHelper.LoadFirstAssetWithName<SoundConfigSO>("SoundConfigSO");

            if (config == null)
            {
                config = CreateInstance<SoundConfigSO>();
                config.OnValidate();

                AssetDatabase.CreateAsset(config, "Assets/SoundConfigSO.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Created SoundConfigSO");
            }

            _instance = config;
            return config;
        }
    }
}
#endif
