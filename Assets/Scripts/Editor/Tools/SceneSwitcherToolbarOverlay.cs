using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace NFramework.Editors
{
    [Overlay(typeof(SceneView), "Scene Switcher", true)]
    public class SceneSwitcherToolbarOverlay : Overlay, ICreateVerticalToolbar, ICreateHorizontalToolbar
    {
        public override VisualElement CreatePanelContent()
        {
            // Create a VisualElement to serve as the root of our UI.
            VisualElement _root = new VisualElement();

            // Create a vertical layout container.
            VisualElement _layout = createContentElements(FlexDirection.Column);

            // Add the vertical layout to the root.
            _root.Add(_layout);
            return _root;
        }

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            OverlayToolbar overlayToolbar = new OverlayToolbar();
            overlayToolbar.Add(createContentElements());
            return overlayToolbar;
        }

        public OverlayToolbar CreateVerticalToolbarContent()
        {
            OverlayToolbar overlayToolbar = new OverlayToolbar();
            overlayToolbar.Add(createContentElements(FlexDirection.Column));
            return overlayToolbar;
        }

        private VisualElement createContentElements(FlexDirection _flexDirection = FlexDirection.Row)
        {
            // Create a layout container.
            VisualElement layout = new VisualElement()
            {
                style =
                {
                    flexDirection = _flexDirection,
                }
            };

            // Create Play button
            Button playButton = new Button(SceneSwitcherControl.PlayFirstScene)
            {
                text = "Play 1st scene"
            };
            layout.Add(playButton);

            // Get the scenes in the Build Settings.
            string[] scenes = SceneSwitcherControl.GetActiveScenesInBuildSetting();

            // Create buttons for each scene.
            foreach (string scene in scenes)
            {
                Button _sceneButton = new Button(() => SceneSwitcherControl.OpenScene(scene))
                {
                    text = scene + " scene"
                };
                layout.Add(_sceneButton);
            }
            return layout;
        }
    }
}

