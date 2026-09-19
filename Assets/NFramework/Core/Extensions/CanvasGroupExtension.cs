using UnityEngine;

namespace NFramework
{
    public static class CanvasGroupExtension
    {
        /// <summary>
        /// Toggle CanvasGroup Alpha, Interactable and BlocksRaycasts settings
        /// </summary>
        public static void SetState(this CanvasGroup canvas, bool isOn)
        {
            canvas.alpha = isOn ? 1 : 0;
            canvas.interactable = isOn;
            canvas.blocksRaycasts = isOn;
        }
    }
}