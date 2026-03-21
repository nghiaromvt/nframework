using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NFramework
{
    public class UIView : MonoBehaviour
    {
#if UNITY_EDITOR
        public static string SavePathPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("UIScriptDefineMenuSavePath");
        public static string ViewsFolderPathPrefsKey => EditorHelper.GetUniqueProjectPrefsKey("UIScriptDefineViewsFolderPath");
        public static string NamespacePrefsKey => EditorHelper.GetUniqueProjectPrefsKey("UIScriptDefineMenuNamespace");
        
        public static string SavePath
        {
            get => EditorPrefs.GetString(SavePathPrefsKey, "");
            set => EditorPrefs.SetString(SavePathPrefsKey, value);
        }
        
        public static string ViewsFolderPath
        {
            get => EditorPrefs.GetString(ViewsFolderPathPrefsKey, "");
            set => EditorPrefs.SetString(ViewsFolderPathPrefsKey, value);
        }
#endif
        
        [ReadOnly] public string defineKeyConstName;
        [OnInspectorInit(nameof(OnKeyChanged)) ,OnValueChanged(nameof(OnKeyChanged))] public string key;
        
        [SerializeField] private UILayer _uiLayer;
        [SerializeField] private bool _pauseGameStatus;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;
        
        private CanvasGroup _canvasGroup;
        
        public UILayer UILayer => _uiLayer;
        public bool PauseGameStatus { get; protected set; }
        public string ID { get; set; }
        public bool IsFromResources { get; set; }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup)
                    _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }
        
        private void OnKeyChanged()
        {
            defineKeyConstName = key.ToValidConstKey();
                
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = $"\u26a0 Key must not be empty!";
                return;
            }
            
            if (string.IsNullOrEmpty(ViewsFolderPath))
            {
                _showError = true;
                _errorMessage = $"\u26a0 No views folder path provided!";
                return;
            }
                
            var prefabs = FileHelper.LoadAssetsWithType<GameObject>("t:Prefab", 
                $"Assets/{ViewsFolderPath}");
            
            foreach (var pf in prefabs)
            {
                if (pf.TryGetComponent<UIView>(out var view))
                {
                    if (view == this) continue;
                    
                    if (view.key == key)
                    {
                        _showError = true;
                        _errorMessage = $"\u26a0 Duplicate key with other view: {view.name}!";
                        return;
                    }
                }
            }
                
            _showError = false;
#endif
        }

        public virtual void OnOpen(UIInputData inputData)
        {
            inputData ??= new UIInputData();
            
            PauseGameStatus = inputData.pauseStatus switch
            {
                UIInputData.EPauseGameStatus.UseDefault => _pauseGameStatus,
                UIInputData.EPauseGameStatus.Pause => true,
                UIInputData.EPauseGameStatus.NoPause => false,
                _ => PauseGameStatus
            };

            if (PauseGameStatus) 
                PauseGameHandler.Pause(this);
        }

        public virtual UIOutputData OnClose()
        {
            if (PauseGameStatus) 
                PauseGameHandler.Unpause(this);
            
            return UIOutputData.Empty;
        }
        
        public UIOutputData CloseSelf(bool destroy = false) => UIManager.Close(this, destroy);
    }
    

    [Serializable]
    public class UIInputData
    {
        public enum EPauseGameStatus { UseDefault, Pause, NoPause }
        
        public EPauseGameStatus pauseStatus;
    }

    [Serializable]
    public class UIOutputData
    {
        public static readonly UIOutputData Empty = new();
    }
}