using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class UIView : MonoBehaviour
    {
        #region Variables

        [ReadOnly] public string defineKeyConstName;
        [ReadOnly] public string key;

        [SerializeField] private UILayer _uiLayer;
        [SerializeField] private bool _pauseGameStatus;

        [HideLabel, ReadOnly, ShowInInspector, ShowIf(nameof(_showError)), GUIColor(1, 0.3f, 0.3f)] 
        private string _errorMessage;
        private bool _showError;

        private CanvasGroup _canvasGroup;
        private bool _initialized;

        #endregion

        #region Properties

        public UILayer UILayer => _uiLayer;
        public bool PauseGameStatus { get; protected set; }
        public string ID { get; private set; }
        public bool IsFromResources { get; private set; }
        public bool IsOpen => UIManager.IsSpecificViewShown(ID, out _);

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup)
                    _canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        #endregion

        #region Unity Functions

#if UNITY_EDITOR
        private string _lastKnownName;

        private void Reset() => RefreshKey();
        
        private void OnValidate()
        {
            if (_lastKnownName != gameObject.name)
                RefreshKey();
        }
#endif

        #endregion

        #region Public Functions

        public virtual void Initialize(string id, bool isFromResources = false)
        {
            if (_initialized) return;
            _initialized = true;
            ID = id;
            IsFromResources = isFromResources;
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

        #endregion

        #region Private Functions

#if UNITY_EDITOR
        private void RefreshKey()
        {
            _lastKnownName = gameObject.name;
            key = gameObject.name;
            defineKeyConstName = key.ToValidConstKey();

            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = "\u26a0 Key must not be empty!";
                return;
            }

            var error = ValidateDuplicateKey(key, defineKeyConstName, this);
            _showError = error != null;
            _errorMessage = error ?? string.Empty;
        }

        /// <summary>
        /// Checks for duplicate key/defineKeyConstName among existing UIView prefabs.
        /// Returns error message if duplicate found, null if valid.
        /// Pass excludeView to skip self-check (used by UIView.RefreshKey).
        /// </summary>
        public static string ValidateDuplicateKey(string viewKey, string constName, UIView excludeView = null)
        {
            var config = NFrameworkConfigSO.GetConfig();

            if (string.IsNullOrEmpty(config.uiViewsFolderPath))
                return "\u26a0 No views folder path provided!";

            var prefabs = FileHelper.LoadAssetsWithType<GameObject>(
                "t:Prefab",
                $"Assets/{config.uiViewsFolderPath}"
            );

            foreach (var pf in prefabs)
            {
                if (!pf.TryGetComponent<UIView>(out var view))
                    continue;

                if (excludeView != null && view == excludeView)
                    continue;

                if (view.key == viewKey)
                    return $"\u26a0 Duplicate key with other view: {view.name}!";

                if (view.defineKeyConstName == constName)
                    return $"\u26a0 Duplicate define key const name with other view: {view.name}!";
            }

            return null;
        }
#endif

        #endregion
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