using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Tools.QuickNav
{
    public class QuickNavEditorModule
    {
        public enum ModuleType
        {
            History,
            Favorites,
        }

        private QuickNavListControl quickNavListControl;
        private Vector2 quickNavListScrollPosition;

        private QuickNavDataManager dataManager;

        private SerializedObject serializedObject;
        private SerializedProperty serializedProperty;
        private List<QuickNavItem> quickNavList;


        public string headerText = "";
        public bool reorderEnabled = false;
        public bool headerVisible = QuickNavSettingsProvider.HeaderVisible;
        public bool toolbarVisible = true;
        public bool addSelectedEnabled = false;
        public ModuleType moduleType;

        // icons depending on the navigation direction
        private GUIContent previousIcon;
        private GUIContent nextIcon;

        public QuickNavEditorModule( QuickNavDataManager dataManager, ModuleType moduleType)
        {
            this.dataManager = dataManager;

            this.moduleType = moduleType;
            this.serializedObject = dataManager.GetSerializedObject();

            switch( moduleType)
            {
                case ModuleType.History:
                    this.serializedProperty = dataManager.GetHistoryProperty(); // history: might need the "quickNavData." prefix depending on what is the parent
                    this.quickNavList = dataManager.GetHistoryList();
                    this.toolbarVisible = QuickNavSettingsProvider.HistoryToolbarVisible;
                    break;

                case ModuleType.Favorites:
                    this.serializedProperty = dataManager.GetFavoritesProperty(); // favorites: might need the "quickNavData." prefix depending on what is the parent
                    this.quickNavList = dataManager.GetFavoritesList();
                    this.toolbarVisible = QuickNavSettingsProvider.FavoritesToolbarVisible;
                    break;
            }

            // setup styles, icons etc
            SetupStyles();
        }

        public ModuleType GetModuleType()
        {
            return moduleType;
        }

        /// <summary>
        /// Setup styles, icons, etc
        /// </summary>
        private void SetupStyles()
        {
            switch (moduleType)
            {
                case ModuleType.History: // fallthrough
                case ModuleType.Favorites:
                    previousIcon = GUIStyles.LeftIcon;
                    nextIcon = GUIStyles.RightIcon;
                    break;

                default: throw new System.Exception($"Unsupported module type: {moduleType}");
            }
        }

        public List<QuickNavItem> GetQuickNavItemList()
        {
            return quickNavList;
        }

        public void OnEnable()
        {
            // initialize UI components
            quickNavListControl = new QuickNavListControl(this, headerText, reorderEnabled, headerVisible, serializedObject, serializedProperty);
        }

        public void OnGUI()
        {
            // EditorGUILayout.LabelField(string.Format("Current QuickNav Index: {0}", currentSelectionHistoryIndex));

            if (toolbarVisible)
            {
                GUILayout.Space(6);

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(previousIcon, GUILayout.Width(80), GUILayout.Height(GUIStyles.TOOLBAR_BUTTON_HEIGHT)))
                    {
                        int selectionIndex = quickNavListControl.Next();

                        JumpToQuickNavItem(true, selectionIndex);
                    }

                    if (GUILayout.Button(nextIcon, GUILayout.Width(80), GUILayout.Height(GUIStyles.TOOLBAR_BUTTON_HEIGHT)))
                    {
                        int selectionIndex = quickNavListControl.Previous();

                        JumpToQuickNavItem(true, selectionIndex);


                    }

                    GUILayout.FlexibleSpace();

                    if (addSelectedEnabled)
                    {

                        if (GUILayout.Button(GUIStyles.AddIcon, GUILayout.Width(60), GUILayout.Height(GUIStyles.TOOLBAR_BUTTON_HEIGHT)))
                        {
                            dataManager.AddSelectedToFavorites();
                        }

                    }

                    if (GUILayout.Button(GUIStyles.ClearIcon, GUILayout.Height(GUIStyles.TOOLBAR_BUTTON_HEIGHT)))
                    {
                        bool performDelete = true;

                        if (QuickNavSettingsProvider.ConfirmDelete)
                        {
                            performDelete = EditorUtility.DisplayDialog("Confirmation", $"Clear list? This action cannot be undone!", "Yes", "No");
                        }

                        if (performDelete)
                        {
                            GetQuickNavItemList().Clear();
                            quickNavListControl.Reset();

                            EditorUtility.SetDirty(serializedObject.targetObject);
                        }
                    }

                }
                GUILayout.EndHorizontal();

                GUILayout.Space(6);
            }

            // show history list
            quickNavListScrollPosition = EditorGUILayout.BeginScrollView(quickNavListScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                quickNavListControl.DoLayoutList();

                // context popup menu
                if (moduleType == ModuleType.Favorites)
                {
                    switch(Event.current.type)
                    {
                        case EventType.ContextClick:
                            Vector2 position = Event.current.mousePosition;
                            Rect popupRect = new Rect(position, Vector2.zero); // 2nd parameter is the offset from mouse position

                            PopupWindow.Show(popupRect, new QuickNavPopupWindow(this, headerText));

                            Event.current.Use();
                            break;

                        case EventType.DragUpdated:
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            break;

                        case EventType.DragPerform:
                            DragAndDrop.AcceptDrag();

                            dataManager.AddToFavorites(DragAndDrop.objectReferences);

                            Event.current.Use();
                            break;
                    }
                }

            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get the number of items in the list
        /// </summary>
        /// <returns></returns>
        public int GetItemCount()
        {
            // TODO: use only the serializedProperty, don't hand over the quicknavlist
            // for some reason we can't access objectReferenceValue from the serializedProperty (might be Unity bug)
            // another thing is that in QuickNavEditorWindow OnSelectionChange is invoked while the object is loadeded
            // eg on scriptchange the array index becomes invalid when an object is selected in the hierarchy and the
            // editorwindow is open; a race condition which can be solved, but not necessarily => handing over the 
            // list is currently working fine and can be solved later, objectReferenceValue would be more important
            return GetQuickNavItemList().Count;
        }

        public QuickNavItem GetCurrentQuickNavItem()
        {
            if (GetItemCount() == 0)
                return null;

            int selectionIndex = quickNavListControl.GetCurrentSelectionIndex();

            if (selectionIndex < 0 || selectionIndex >= GetItemCount())
                return null;

            QuickNavItem quickNavItem = GetQuickNavItemList()[selectionIndex];

            return quickNavItem;
        }

        /// <summary>
        /// Get the current quick nav item and jump to it by selecting it
        /// </summary>
        public void JumpToQuickNavItem( bool openInInspector, int selectionIndex)
        {
            QuickNavItem quickNavItem = GetCurrentQuickNavItem();

            if (quickNavItem == null)
                return;

            // select in reorderable list
            quickNavListControl.index = selectionIndex;
            //reorderableList.Select(currentSelectionIndex);

            // select the object and open it in the inspector
            if (openInInspector)
            {
                // selection objects
                UnityEngine.Object[] objects = new UnityEngine.Object[] { quickNavItem.unityObject };

                // select objects
                Selection.objects = objects;
            }
            // just select the object, don't open it in the inspector
            else
            {
                
                EditorGUIUtility.PingObject(quickNavItem.unityObject);

                // alternative: open in application (eg doubleclick)
                // AssetDatabase.OpenAsset(quickNavItem.unityObject);

            }

        }

        public QuickNavDataManager GetDataManager()
        {
            return dataManager;
        }

        /// <summary>
        /// Check if the currently selected item matches the first in the list.
        /// This is used with the history module in order to not add tuplicates to the FIFO queue.
        /// </summary>
        /// <returns></returns>
        public bool CurrentSelectionMatchesFirstItem()
        {
            if (Selection.instanceIDs.Length == 1)
            {
                QuickNavItem quickNavItem = GetCurrentQuickNavItem();
                if (quickNavItem != null && Selection.objects.Length > 0)
                {
                    if (quickNavItem.unityObject == Selection.objects[0])
                        return true;
                }
            }

            return false;
        }
    }
}
