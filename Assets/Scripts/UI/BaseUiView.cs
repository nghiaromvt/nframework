using System;
using UnityEngine;

namespace NFramework
{
    public class BaseUiView : MonoBehaviour
    {
        [SerializeField] private EUiLayer _uiLayer;
        [SerializeField] private bool _defaultPauseGameStatus;

        private CanvasGroup _canvasGroup;
        
        public EUiLayer UiLayer => _uiLayer;
        public bool PauseGameStatus { get; protected set; }
        public string Id { get; set; }
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

        public virtual void OnOpen(BaseUiInputData inputData)
        {
            PauseGameStatus = inputData.pauseStatus switch
            {
                BaseUiInputData.EPauseGameStatus.UseDefault => _defaultPauseGameStatus,
                BaseUiInputData.EPauseGameStatus.Pause => true,
                BaseUiInputData.EPauseGameStatus.NoPause => false,
                _ => PauseGameStatus
            };

            if (PauseGameStatus) 
                PauseGameHandler.Pause(this);
        }

        public virtual BaseUiOutputData OnClose()
        {
            if (PauseGameStatus) 
                PauseGameHandler.Unpause(this);
            
            return BaseUiOutputData.Empty;
        }
        
        public BaseUiOutputData CloseSelf(bool destroy = false) => UIManager.Close(this, destroy);
    }
    

    [Serializable]
    public class BaseUiInputData
    {
        public enum EPauseGameStatus { UseDefault, Pause, NoPause }
        
        public EPauseGameStatus pauseStatus;
    }

    [Serializable]
    public class BaseUiOutputData
    {
        public static readonly BaseUiOutputData Empty = new();
    }
}