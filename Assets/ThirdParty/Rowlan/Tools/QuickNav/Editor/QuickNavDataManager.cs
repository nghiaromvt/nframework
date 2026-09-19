using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Tools.QuickNav
{
    public class QuickNavDataManager
    {
        [SerializeField]
        public QuickNavData quickNavData;

        private SerializedObject serializedObject;

        /// <summary>
        /// Toggle for edit mode. User can override the title of the displayed gameobject name
        /// </summary>
        private bool editModeEnabled = false;

        public void OnEnable()
        {
            ScriptableObjectManager<QuickNavData> settingsManager = new ScriptableObjectManager<QuickNavData>(ProjectSetup.SETTINGS_FOLDER, ProjectSetup.SETTINGS_FILENAME);
            quickNavData = settingsManager.GetAsset();

            serializedObject = new SerializedObject(quickNavData);
        }

        public void InitializeHistory()
        {
            // clear history at startup
            quickNavData.history.Clear();

            EditorUtility.SetDirty(quickNavData);
        }

        public void Refresh()
        {
            quickNavData.Refresh();
        }

        public SerializedObject GetSerializedObject()
        {
            return serializedObject;
        }

        public void AddToFavorites(UnityEngine.Object[] objects)
        {
            if (objects == null)
                return;

            foreach (UnityEngine.Object unityObject in objects)
            {
                // add the selected object
                // check if the object is null; would eg be the case of the scene object in the hierarchy
                if (unityObject != null)
                {
                    QuickNavItem navItem = CreateQuickNavItem(unityObject);

                    quickNavData.favorites.Add(navItem);
                }
            }

            // persist the changes
            EditorUtility.SetDirty(quickNavData);
        }

        public QuickNavItem CreateQuickNavItem(UnityEngine.Object unityObject)
        {
            string guid;
            long localId;

            bool isProjectContext = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(unityObject, out guid, out localId);

            QuickNavItem navItem = new QuickNavItem(unityObject, isProjectContext);

            return navItem;
        }

        public void AddToFavorites(QuickNavItem quickNavItem)
        {
            quickNavData.favorites.Add(quickNavItem);
        }

        public void ToggleEditMode()
        {
            editModeEnabled = !editModeEnabled;
        }

        public bool IsEditModeEnabled() 
        { 
            return editModeEnabled;
        }

        public void AddSeparatorToFavorites()
        {
            QuickNavItem separator = QuickNavItem.CreateSeparator();

            quickNavData.favorites.Add(separator);

            EditorUtility.SetDirty(quickNavData);
        }

        public void ClampHistorySize()
        {
            int itemsMax = quickNavData.historyItemsMax;

            // ensure collection doesn't exceed max size
            if (quickNavData.history.Count >= itemsMax)
            {
                quickNavData.history.RemoveRange(itemsMax - 1, quickNavData.history.Count - itemsMax + 1);
            }
        }

        public void InsertHistoryItem(UnityEngine.Object unityObject)
        {
            // add the selected object to history
            // check if the object is null; would eg be the case of the scene object in the hierarchy
            if (unityObject == null)
                return;

            QuickNavItem navItem = CreateQuickNavItem(unityObject);

            // insert new items first
            quickNavData.history.Insert(0, navItem);

            // persist the changes
            EditorUtility.SetDirty(quickNavData);
        }

        public SerializedProperty GetHistoryProperty()
        {
            SerializedProperty property = serializedObject.FindProperty("history");
            return property;
        }

        public SerializedProperty GetFavoritesProperty()
        {
            SerializedProperty property = serializedObject.FindProperty("favorites");
            return property;
        }

        public List<QuickNavItem> GetHistoryList()
        {
            return quickNavData.history;
        }

        public List<QuickNavItem> GetFavoritesList()
        {
            return quickNavData.favorites;
        }

        public bool IsFavoritesItem(UnityEngine.Object unityObject)
        {
            return quickNavData.IsFavoritesItem(unityObject);
        }

        #region Selection related

        public void AddSelectedToFavorites()
        {
            AddToFavorites(Selection.objects);
        }

        /// <summary>
        /// Add to history if a single item got selected
        /// </summary>
        public void AddSelectedToHistory()
        {
            // single item selection / navigation
            if (Selection.objects.Length != 1)
                return;

            // ensure collection doesn't exceed max size
            ClampHistorySize();

            // add to history if a single item got selected
            UnityEngine.Object selectedUnityObject = Selection.objects[0];

            InsertHistoryItem(selectedUnityObject);
        }

        #endregion Selection related
    }
}