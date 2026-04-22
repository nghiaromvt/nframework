using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class UIView : MonoBehaviour
    {
        [ReadOnly] public string defineKeyConstName;
        [ReadOnly] public string key;

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

#if UNITY_EDITOR
        private void Reset() => RefreshKey();
        
        private void OnValidate() => RefreshKey();

        private void RefreshKey()
        {
            key = gameObject.name;
            defineKeyConstName = key.ToValidConstKey();

            if (string.IsNullOrEmpty(key))
            {
                _showError = true;
                _errorMessage = "\u26a0 Key must not be empty!";
                return;
            }

            var config = NFrameworkConfigSO.GetConfig();

            if (string.IsNullOrEmpty(config.uiViewsFolderPath))
            {
                _showError = true;
                _errorMessage = "\u26a0 No views folder path provided!";
                return;
            }

            var prefabs = FileHelper.LoadAssetsWithType<GameObject>(
                "t:Prefab",
                $"Assets/{config.uiViewsFolderPath}"
            );

            foreach (var pf in prefabs)
            {
                if (!pf.TryGetComponent<UIView>(out var view))
                    continue;

                if (view == this)
                    continue;

                if (view.key == key)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate key with other view: {view.name}!";
                    return;
                }
                
                if (view.defineKeyConstName == defineKeyConstName)
                {
                    _showError = true;
                    _errorMessage = $"\u26a0 Duplicate define key const name with other view: {view.name}!";
                    return;
                }
            }

            _showError = false;
            _errorMessage = string.Empty;
        }
#endif

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