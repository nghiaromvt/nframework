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
            VisualElement root = new VisualElement();

            // Create a vertical layout container.
            VisualElement layout = CreateContentElements(FlexDirection.Column);

            // Add the vertical layout to the root.
            root.Add(layout);
            return root;
        }

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            OverlayToolbar overlayToolbar = new OverlayToolbar();
            overlayToolbar.Add(CreateContentElements());
            return overlayToolbar;
        }

        public OverlayToolbar CreateVerticalToolbarContent()
        {
            OverlayToolbar overlayToolbar = new OverlayToolbar();
            overlayToolbar.Add(CreateContentElements(FlexDirection.Column));
            return overlayToolbar;
        }

        private VisualElement CreateContentElements(FlexDirection flexDirection = FlexDirection.Row)
        {
            // Create a layout container.
            VisualElement layout = new VisualElement()
            {
                style =
                {
                    flexDirection = flexDirection,
                }
            };

            // Create Play button
            Button playButton = new Button(SceneSwitcherControl.PlayGame)
            {
                text = "Play Game"
            };
            layout.Add(playButton);

            // Get the scenes in the Build Settings.
            string[] scenes = SceneSwitcherControl.GetActiveScenesInBuildSetting();

            // Create buttons for each scene.
            foreach (string scene in scenes)
            {
                Button sceneButton = new Button(() => SceneSwitcherControl.OpenScene(scene))
                {
                    text = scene.ToString(),
                };
                layout.Add(sceneButton);
            }
            return layout;
        }
    }
}

