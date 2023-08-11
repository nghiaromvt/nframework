using UnityEngine;

namespace NFramework
{
    public class BaseUIView : MonoBehaviour
    {
        [SerializeField] private EUILayer _uiLayer;
        [SerializeField] private bool _isUnique = true;
        [SerializeField] private bool _canDestroy;

        public EUILayer UILayer => _uiLayer;
        public bool IsUnique => _isUnique;
        public bool CanDestroy => _canDestroy;
        public string Identifier { get; set; }

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    if (TryGetComponent(out CanvasGroup canvasGroup))
                        _canvasGroup = canvasGroup;
                    else
                        _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        public virtual void CloseSelf(bool destroy = false) => UIManager.I.Close(this, destroy);

        public virtual void HandleOnKeyBack() { }

        public virtual void OnOpen() { }

        public virtual void OnClose() { }
    }
}


