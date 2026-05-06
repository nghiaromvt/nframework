using System;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace NFramework.Editor
{
    [Serializable]
    public class UIViewCreator
    {
        #region Variables

        [SerializeField, Required, OnInspectorInit(nameof(OnViewNameChanged)), OnValueChanged(nameof(OnViewNameChanged))]
        private string _viewName = "NewView";

        [ReadOnly, SerializeField]
        private string _defineKeyConstName;

        [ReadOnly, SerializeField, HorizontalGroup("PrefabPath")]
        private string _prefabViewPath;

        [HorizontalGroup("PrefabPath", Width = 50), Button("Edit")]
        private void LocateConfig()
        {
            var config = NFrameworkConfigSO.GetConfig();
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
        }

        [SerializeField, OnValueChanged(nameof(OnViewNameChanged))]
        private string _scriptName;

        [SerializeField, FolderPath(RequireExistingPath = true, ParentFolder = "Assets")]
        private string _scriptFolderPath = "";

        [SerializeField] private UILayer _uiLayer = UILayer.Popup;

        [Header("Script Define")]
        [SerializeField] private bool _generateScriptDefine = true;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)]
        private string _errorMessage;
        private bool _showError;

        #endregion

        #region Private Functions

        private void OnViewNameChanged()
        {
            if (string.IsNullOrEmpty(_scriptName) || _scriptName == _lastAutoScriptName)
                _scriptName = _viewName;

            _lastAutoScriptName = _scriptName;
            _defineKeyConstName = _viewName.ToValidConstKey();
            
            var config = NFrameworkConfigSO.GetConfig();
            _prefabViewPath = config != null && !string.IsNullOrEmpty(config.uiViewsFolderPath)
                ? $"Assets/{config.uiViewsFolderPath}"
                : "(Not configured)";
            
            ValidateInputs();
        }

        private string _lastAutoScriptName;

        private void ValidateInputs()
        {
            if (string.IsNullOrEmpty(_viewName))
            {
                _showError = true;
                _errorMessage = "\u26a0 View name must not be empty!";
                return;
            }

            if (string.IsNullOrEmpty(_scriptName))
            {
                _showError = true;
                _errorMessage = "\u26a0 Script name must not be empty!";
                return;
            }

            var config = NFrameworkConfigSO.GetConfig();

            if (string.IsNullOrEmpty(config.uiViewsFolderPath))
            {
                _showError = true;
                _errorMessage = "\u26a0 No UI Views Folder Path provided in NFrameworkConfigSO!";
                return;
            }

            // Check duplicate key/defineKeyConstName
            var duplicateError = UIView.ValidateDuplicateKey(_viewName, _defineKeyConstName);
            if (duplicateError != null)
            {
                _showError = true;
                _errorMessage = duplicateError;
                return;
            }

            // Check duplicate script file
            var scriptPath = Path.Combine(Application.dataPath, _scriptFolderPath, $"{_scriptName}.cs");
            if (File.Exists(scriptPath))
            {
                _showError = true;
                _errorMessage = $"\u26a0 Script already exists: {_scriptName}.cs!";
                return;
            }

            _showError = false;
            _errorMessage = string.Empty;
        }

        private string GenerateScriptContent()
        {
            var nameSpace = NFrameworkConfigSO.GetConfig().scriptDefineNamespace;
            var sb = new StringBuilder();

            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using NFramework;");
            sb.AppendLine();

            var indent = "";
            if (!string.IsNullOrWhiteSpace(nameSpace))
            {
                sb.AppendLine($"namespace {nameSpace}");
                sb.AppendLine("{");
                indent = "    ";
            }

            sb.AppendLine($"{indent}public class {_scriptName} : UIView");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent}    public override void Initialize(string id, bool isFromResources = false)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        base.Initialize(id, isFromResources);");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override void OnOpen(UIInputData inputData)");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        base.OnOpen(inputData);");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine();
            sb.AppendLine($"{indent}    public override UIOutputData OnClose()");
            sb.AppendLine($"{indent}    {{");
            sb.AppendLine($"{indent}        return base.OnClose();");
            sb.AppendLine($"{indent}    }}");
            sb.AppendLine($"{indent}}}");

            if (!string.IsNullOrWhiteSpace(nameSpace))
                sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Button

        [Button(ButtonSizes.Gigantic), GUIColor(0.4f, 0.8f, 0.4f)]
        private void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "Cannot create views while in Play Mode.", "OK");
                return;
            }

            ValidateInputs();
            if (_showError)
            {
                EditorUtility.DisplayDialog("Error", _errorMessage, "OK");
                return;
            }

            var config = NFrameworkConfigSO.GetConfig();

            // 1. Create script file
            var scriptDir = Path.Combine(Application.dataPath, _scriptFolderPath);
            if (!Directory.Exists(scriptDir))
                Directory.CreateDirectory(scriptDir);

            var scriptFullPath = Path.Combine(scriptDir, $"{_scriptName}.cs");
            File.WriteAllText(scriptFullPath, GenerateScriptContent());

            // 2. Find UIManager in scene
            var uiManager = UnityEngine.Object.FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("Cannot create view: no UIManager found in the current scene.");
                return;
            }

            var canvas = uiManager.RootCanvas;

            // Create temp GO to save as prefab
            var go = new GameObject(_viewName, typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rectTf = go.GetComponent<RectTransform>();
            rectTf.anchorMin = Vector2.zero;
            rectTf.anchorMax = Vector2.one;
            rectTf.offsetMin = Vector2.zero;
            rectTf.offsetMax = Vector2.zero;

            var prefabDir = $"Assets/{config.uiViewsFolderPath}";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                var parts = prefabDir.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = $"{current}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            var prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{prefabDir}/{_viewName}.prefab");
            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);

            // Instantiate as prefab instance in scene (connected to asset)
            var prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPrefab, canvas.transform);
            var instanceRect = prefabInstance.GetComponent<RectTransform>();
            instanceRect.anchorMin = Vector2.zero;
            instanceRect.anchorMax = Vector2.one;
            instanceRect.offsetMin = Vector2.zero;
            instanceRect.offsetMax = Vector2.zero;

            // 3. Store pending attachment info (survives domain reload)
            EditorPrefs.SetString(PREF_PENDING_PREFAB, prefabPath);
            EditorPrefs.SetString(PREF_PENDING_SCRIPT, _scriptName);
            EditorPrefs.SetInt(PREF_PENDING_LAYER, (int)_uiLayer);
            EditorPrefs.SetBool(PREF_PENDING_SCRIPTDEFINE, _generateScriptDefine);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created UIView: {_viewName}\n  Script: Assets/{_scriptFolderPath}/{_scriptName}.cs\n  Prefab: {prefabPath}");
            Selection.activeGameObject = prefabInstance;
        }

        #endregion

        #region Auto Attach After Compile

        private const string PREF_PENDING_PREFAB = "UIViewCreator_PendingPrefab";
        private const string PREF_PENDING_SCRIPT = "UIViewCreator_PendingScript";
        private const string PREF_PENDING_LAYER = "UIViewCreator_PendingLayer";
        private const string PREF_PENDING_SCRIPTDEFINE = "UIViewCreator_PendingScriptDefine";

        [InitializeOnLoadMethod]
        private static void OnScriptsReloaded()
        {
            var prefabPath = EditorPrefs.GetString(PREF_PENDING_PREFAB, "");
            var scriptName = EditorPrefs.GetString(PREF_PENDING_SCRIPT, "");

            if (string.IsNullOrEmpty(prefabPath) || string.IsNullOrEmpty(scriptName))
                return;

            var uiLayer = (UILayer)EditorPrefs.GetInt(PREF_PENDING_LAYER, 0);
            var generateScriptDefine = EditorPrefs.GetBool(PREF_PENDING_SCRIPTDEFINE, false);

            // Clear pending
            EditorPrefs.DeleteKey(PREF_PENDING_PREFAB);
            EditorPrefs.DeleteKey(PREF_PENDING_SCRIPT);
            EditorPrefs.DeleteKey(PREF_PENDING_LAYER);
            EditorPrefs.DeleteKey(PREF_PENDING_SCRIPTDEFINE);

            // Find script type
            var nameSpace = NFrameworkConfigSO.GetConfig().scriptDefineNamespace;
            var fullTypeName = string.IsNullOrWhiteSpace(nameSpace) ? scriptName : $"{nameSpace}.{scriptName}";

            Type scriptType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                scriptType = assembly.GetType(fullTypeName) ?? assembly.GetType(scriptName);
                if (scriptType != null) break;
            }

            if (scriptType == null || !typeof(UIView).IsAssignableFrom(scriptType))
            {
                Debug.LogError($"Failed to attach script: type {scriptName} not found or doesn't inherit UIView");
                return;
            }

            // Modify prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return;

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null) return;

            if (!instance.GetComponent(scriptType))
            {
                instance.AddComponent(scriptType);

                var view = instance.GetComponent<UIView>();
                if (view != null)
                {
                    var so = new SerializedObject(view);
                    var layerProp = so.FindProperty("_uiLayer");
                    if (layerProp != null)
                    {
                        layerProp.enumValueIndex = (int)uiLayer;
                        so.ApplyModifiedPropertiesWithoutUndo();
                    }

                    // Set key fields so UIDefine generation picks them up
                    view.key = instance.name;
                    view.defineKeyConstName = instance.name.ToValidConstKey();
                    EditorUtility.SetDirty(view);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (generateScriptDefine)
            {
                EditorApplication.delayCall += () => UIScriptDefineEditor.GenerateScriptDefine();
            }
        }

        #endregion
    }
}
