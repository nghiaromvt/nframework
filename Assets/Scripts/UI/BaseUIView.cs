using System;
using UnityEngine;

namespace NFramework
{
    public class BaseUIView : MonoBehaviour
    {
        [SerializeField] private UILayer _uiLayer;
        [SerializeField] private bool _defaultPauseGameStatus;

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

        public virtual void OnOpen(BaseUIInputData inputData)
        {
            PauseGameStatus = inputData.pauseStatus switch
            {
                BaseUIInputData.EPauseGameStatus.UseDefault => _defaultPauseGameStatus,
                BaseUIInputData.EPauseGameStatus.Pause => true,
                BaseUIInputData.EPauseGameStatus.NoPause => false,
                _ => PauseGameStatus
            };

            if (PauseGameStatus) 
                PauseGameHandler.Pause(this);
        }

        public virtual BaseUIOutputData OnClose()
        {
            if (PauseGameStatus) 
                PauseGameHandler.Unpause(this);
            
            return BaseUIOutputData.Empty;
        }
        
        public BaseUIOutputData CloseSelf(bool destroy = false) => UIManager.Close(this, destroy);
    }
    

    [Serializable]
    public class BaseUIInputData
    {
        public enum EPauseGameStatus { UseDefault, Pause, NoPause }
        
        public EPauseGameStatus pauseStatus;
    }

    [Serializable]
    public class BaseUIOutputData
    {
        public static readonly BaseUIOutputData Empty = new();
    }
}