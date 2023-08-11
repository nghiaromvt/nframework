using UnityEngine;
#if UNITY_EDITOR
#endif

namespace NFramework
{
    /// <summary>
    /// Add this class to an empty game object in your scene and it'll let you take screenshots (meant to be used in Editor)
    /// </summary>
    public class ScreenshotHandler : MonoBehaviour
    {
        /// the method to use to take the screenshot. Screencapture uses the API of the same name, and will let you keep 
        /// whatever ratio the game view has, RenderTexture renders to a texture of the specified resolution
        public enum Methods { ScreenCapture, RenderTexture }

        [Header("Screenshot")]
        /// the selected method to take a screenshot with. 
        public Methods method = Methods.ScreenCapture;
        /// the shortcut to watch for to take screenshots
        public KeyCode screenshotShortcut = KeyCode.K;

        /// the size by which to multiply the game view when taking the screenshot
        [ConditionalField(nameof(method), compareValues: Methods.ScreenCapture)]
        public int gameViewSizeMultiplier = 3;

        /// the camera to use to take the screenshot with
        [ConditionalField(nameof(method), compareValues: Methods.RenderTexture)]
        public Camera targetCamera;
        /// the width of the desired screenshot
        [ConditionalField(nameof(method), compareValues: Methods.RenderTexture)]
        public int resolutionWidth;
        /// the height of the desired screenshot
        [ConditionalField(nameof(method), compareValues: Methods.RenderTexture)]
        public int resolutionHeight;

        private string _folderPath;

        /// <summary>
        /// At late update, we look for input
        /// </summary>
        private void LateUpdate()
        {
            if (Input.GetKeyDown(screenshotShortcut))
                TakeScreenshot();
        }

        /// <summary>
        /// Takes a screenshot using the specified method and outputs a console log
        /// </summary>
        [ButtonMethod]
        private void TakeScreenshot()
        {
#if UNITY_EDITOR
            _folderPath = PathUtils.GetScreenShotFolderPath();
            string savePath = "";
            switch (method)
            {
                case Methods.ScreenCapture:
                    savePath = TakeScreenCaptureScreenshot();
                    break;
                case Methods.RenderTexture:
                    savePath = TakeRenderTextureScreenshot();
                    break;
            }
            Logger.Log("Screenshot taken and saved at " + savePath, this);
#endif
        }

        /// <summary>
        /// Takes a screenshot using the ScreenCapture API and saves it to file
        /// </summary>
        private string TakeScreenCaptureScreenshot()
        {
            float width = Screen.width * gameViewSizeMultiplier;
            float height = Screen.height * gameViewSizeMultiplier;
            string savePath = _folderPath + "/screenshot_" + width + "x" + height + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            ScreenCapture.CaptureScreenshot(savePath, gameViewSizeMultiplier);
            return savePath;
        }

        /// <summary>
        /// Takes a screenshot using a render texture and saves it to file
        /// </summary>
        private string TakeRenderTextureScreenshot()
        {
            if (!Application.isPlaying)
            {
                Logger.LogError("Cannot TakeRenderTextureScreenshot when not playing", this);
                return string.Empty;
            }

            string savePath = _folderPath + "/screenshot_" + resolutionWidth + "x" + resolutionHeight + "_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";

            RenderTexture renderTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
            targetCamera.targetTexture = renderTexture;
            Texture2D screenShot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            targetCamera.Render();
            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, resolutionWidth, resolutionHeight), 0, 0);
            targetCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(savePath, bytes);

            return savePath;
        }
    }
}
