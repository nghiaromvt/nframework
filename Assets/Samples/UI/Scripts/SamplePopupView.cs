using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NFramework.UI;

namespace NFramework.UI.Sample
{
    public class SamplePopupView : UIView<SamplePopupView.InputData>
    {
        public class InputData : UIInputData
        {
            public string Title;
            public string Message;
        }

        #region Fields

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _closeButton;

        #endregion

        #region Unity Lifecycle Functions

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        #endregion

        #region Custom Methods

        protected override void OnViewOpened(InputData data)
        {
            base.OnViewOpened(data);

            if (data == null) return;

            if (!string.IsNullOrEmpty(data.Title))
            {
                _titleText.text = data.Title;
            }

            if (!string.IsNullOrEmpty(data.Message))
            {
                _messageText.text = data.Message;
            }
        }

        private void OnCloseButtonClicked()
        {
            CloseSelf().Forget();
        }

        #endregion
    }
}
