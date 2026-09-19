using UnityEditor;
using UnityEngine;
using static Rowlan.Tools.QuickNav.QuickNavEditorWindow;

namespace Rowlan.Tools.QuickNav
{
    public class QuickNavSettingsProvider : SettingsProvider
    {
        const string k_menu = "Rowlan/QuickNav";
        const SettingsScope k_scope = SettingsScope.User;

        // registry keys
        const string k_headerVisible = "Rowlan.QuickNav.HeaderVisible";
        const string k_historyToolbarVisible = "Rowlan.QuickNav.History.ToolbarVisible";
        const string k_favoritesToolbarVisible = "Rowlan.QuickNav.Favorites.ToolbarVisible";
        const string k_confirmDelete = "Rowlan.QuickNav.Confirm.Delete";

        const string k_initialTab = "Rowlan.QuickNav.InitialTab";
        const string k_windowPositionX = "Rowlan.QuickNav.Window.Position.X";
        const string k_windowPositionY = "Rowlan.QuickNav.Window.Positin.Y";
        const string k_windowWidth = "Rowlan.QuickNav.Window.Width";
        const string k_windowHeight = "Rowlan.QuickNav.Window.Height";

        public static bool HeaderVisible
        {
            get { return EditorPrefs.GetBool(k_headerVisible, true); }
            set { EditorPrefs.SetBool(k_headerVisible, value); }
        }

        public static bool HistoryToolbarVisible
        {
            get { return EditorPrefs.GetBool(k_historyToolbarVisible, true); }
            set { EditorPrefs.SetBool(k_historyToolbarVisible, value); }
        }

        public static bool FavoritesToolbarVisible
        {
            get { return EditorPrefs.GetBool(k_favoritesToolbarVisible, true); }
            set { EditorPrefs.SetBool(k_favoritesToolbarVisible, value); }
        }

        public static bool ConfirmDelete
        {
            get { return EditorPrefs.GetBool(k_confirmDelete, false); }
            set { EditorPrefs.SetBool(k_confirmDelete, value); }
        }

        public static int InitialTab
        {
            get { return EditorPrefs.GetInt(k_initialTab, (int) QuickNavTab.History); }
            set { EditorPrefs.SetInt(k_initialTab, value); }
        }
        public static int WindowPositionX
        {
            get { return EditorPrefs.GetInt(k_windowPositionX, 0); }
            set { EditorPrefs.SetInt(k_windowPositionX, value); }
        }

        public static int WindowPositionY
        {
            get { return EditorPrefs.GetInt(k_windowPositionY, 0); }
            set { EditorPrefs.SetInt(k_windowPositionY, value); }
        }

        public static int WindowWidth
        {
            get { return EditorPrefs.GetInt(k_windowWidth, 400); }
            set { EditorPrefs.SetInt(k_windowWidth, value); }
        }

        public static int WindowHeight
        {
            get { return EditorPrefs.GetInt(k_windowHeight, 600); }
            set { EditorPrefs.SetInt(k_windowHeight, value); }
        }

        public QuickNavSettingsProvider(string menuPath, SettingsScope scope) : base(menuPath, scope)
        {
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            // reset button
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reset"))
                {
                    HeaderVisible = true;
                    HistoryToolbarVisible = true;
                    FavoritesToolbarVisible = true;
                    ConfirmDelete = false;
                    InitialTab = (int)QuickNavTab.History;
                    WindowPositionX = 0;
                    WindowPositionY = 0;
                    WindowWidth = 400;
                    WindowHeight = 600;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Please close and re-open QuickNav for your settings changes to have effect", MessageType.Info);

            // content browser
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField("QuickNav", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                bool headerVisibleValue = EditorGUILayout.Toggle("Header Visible", HeaderVisible);
                bool historyToolbarVisibleValue = EditorGUILayout.Toggle("History Toolbar", HistoryToolbarVisible);
                bool favoritesToolbarVisibleValue = EditorGUILayout.Toggle("Favorites Toolbar", FavoritesToolbarVisible);
                bool confirmDeleteValue = EditorGUILayout.Toggle("Confirm Delete", ConfirmDelete);

                QuickNavTab initialTabValue = (QuickNavTab)EditorGUILayout.EnumPopup("Initial Tab", (QuickNavTab)InitialTab);

                RectInt windowRect = new RectInt(WindowPositionX, WindowPositionY, WindowWidth, WindowHeight);
                windowRect = EditorGUILayout.RectIntField("Window Dimensions", windowRect);

                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();

                if (check.changed)
                {
                    HeaderVisible = headerVisibleValue;
                    HistoryToolbarVisible = historyToolbarVisibleValue;
                    FavoritesToolbarVisible = favoritesToolbarVisibleValue;
                    ConfirmDelete = confirmDeleteValue;
                    InitialTab = (int)initialTabValue;
                    WindowPositionX = windowRect.x;
                    WindowPositionY = windowRect.y;
                    WindowWidth = windowRect.width;
                    WindowHeight = windowRect.height;
                }
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new QuickNavSettingsProvider(k_menu, k_scope);
        }
    }
}