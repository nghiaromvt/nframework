using UnityEngine;

namespace NFramework
{
    public class BaseUIView : MonoBehaviour
    {
        // Only set in prefab, cannot change runtime
        [SerializeField] private EUILayer _uiLayer;
        [SerializeField] private bool _pauseGame;

        private CanvasGroup _canvasGroup;
        
        public EUILayer UILayer => _uiLayer;
        public bool PauseGame => _pauseGame;
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

        public void CloseSelf(bool destroy = false) => UIManager.Close(this, destroy);

        public virtual void OnOpen()
        {
        }

        public virtual void OnClose()
        {
        }
    }
}