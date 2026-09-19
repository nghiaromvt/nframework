using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NFramework.UI;

namespace NFramework.UI.Sample
{
    public class SampleMenuView : UIView
    {
        #region Fields

        [SerializeField] private Button _openPopupButton;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _statusText;

        private int _actionClickCount;

        #endregion

        #region Unity Lifecycle Functions

        private void Awake()
        {
            _openPopupButton.onClick.AddListener(OnOpenPopupButtonClicked);
            _actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        private void OnDestroy()
        {
            _openPopupButton.onClick.RemoveListener(OnOpenPopupButtonClicked);
            _actionButton.onClick.RemoveListener(OnActionButtonClicked);
        }

        #endregion

        #region Custom Methods

        protected override void OnViewOpened()
        {
            base.OnViewOpened();
            _actionClickCount = 0;
            _statusText.text = "Menu ready. Click a button to test.";
        }

        private void OnOpenPopupButtonClicked()
        {
            UIManager.OpenResources<SamplePopupView>(UIDefine.SAMPLE_POPUP_VIEW, new SamplePopupView.InputData
            {
                Title = "Notification",
                Message = "Hello from SampleMenuView!\nThis is a sample popup dialog."
            });
        }

        private void OnActionButtonClicked()
        {
            _actionClickCount++;
            _statusText.text = $"Action button clicked {_actionClickCount} time{(_actionClickCount > 1 ? "s" : "")}.";
        }

        #endregion
    }
}
