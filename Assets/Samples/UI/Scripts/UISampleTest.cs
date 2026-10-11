using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using NFramework.UI;

namespace NFramework.UI.Sample
{
    public class UISampleTest : MonoBehaviour
    {
        #region Unity Lifecycle Functions

        private void Start()
        {
            if (UIManager.HasInstance)
            {
                OpenMenu();
            }
        }

        #endregion

        #region Custom Methods

        [Button(ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 0.4f)]
        public void OpenMenu()
        {
            UIManager.OpenResources<SampleMenuView>(UIDefine.SAMPLE_MENU_VIEW);
        }

        [Button(ButtonSizes.Medium), GUIColor(0.3f, 0.7f, 1f)]
        public void OpenPopup()
        {
            UIManager.OpenResources<SamplePopupView>(UIDefine.SAMPLE_POPUP_VIEW, new SamplePopupView.InputData
            {
                Title = "Test Popup",
                Message = "Popup opened directly via UISampleTest."
            });
        }

        [Button(ButtonSizes.Medium), GUIColor(0.8f, 0.5f, 1f)]
        public void OpenScalePopup()
        {
            UIManager.OpenResources<SampleScalePopupView>(UIDefine.SAMPLE_SCALE_POPUP_VIEW, new SampleScalePopupView.InputData
            {
                Title = "Scale Popup",
                Message = "Popup with ScaleViewAnimation opened via UISampleTest."
            });
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.2f)]
        public void ClosePopup()
        {
            UIManager.CloseCurrentInLayer(UILayer.Popup).Forget();
        }

        [Button(ButtonSizes.Medium), GUIColor(1f, 0.4f, 0.4f)]
        public void CloseAll()
        {
            UIManager.CloseAll().Forget();
        }

        #endregion
    }
}
