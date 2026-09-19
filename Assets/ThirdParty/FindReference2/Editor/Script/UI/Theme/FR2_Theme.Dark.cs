using UnityEngine;

namespace vietlabs.fr2
{
    partial class FR2_Theme
    {
        private static FR2_Theme CreateDarkTheme()
        {
            return new FR2_Theme
            {
                SelectionHighlight = new Color(0.2745f, 0.4902f, 0.7451f, 1f), // Unity's dark theme selection
                SelectionHighlightInactive = new Color(0.3f, 0.3f, 0.3f, 1f),
                ErrorColor = new Color(0.8235f, 0.1333f, 0.1333f, 1f),
                WarningColor = new Color(0.9569f, 0.7373f, 0.0078f, 1f),
                InfoColor = new Color(0.2980f, 0.4941f, 1.0f, 1f),
                SuccessColor = new Color(0.0f, 0.8f, 0.0f, 1f),
                SceneHighlight = new Color32(72, 150, 191, 255),
                SeparatorLine = Color.black,
                DirtyIndicator = new Color(0.8f, 0.7f, 0.0f, 1f), // Dark yellow for dark theme
                SecondaryTextColor = new Color(0.7f, 0.7f, 0.7f, 1f), // Fully opaque gray text for paths and extensions in dark theme
                IconColor = Color.white * 0.9f, // Bright icons for dark theme
                IconActiveColor = Color.white, // Full white for active icons in dark theme
                PanelBackground = new Color(0.22f, 0.22f, 0.22f, 1f), // Dark panel background
                PanelTitleTextColor = new Color(0.9f, 0.9f, 0.9f, 1f), // Bright white text for dark theme
                ProjectPanelBackground = new Color(0.22f, 0.22f, 0.22f, .8f) // Dark theme project panel background
            };
        }
    }
}